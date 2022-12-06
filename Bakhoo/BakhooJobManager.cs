using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bakhoo;

internal class BakhooJobManager : BackgroundService
{
    private record WorkerContext(
        IServiceScope Scope,
        IBakhooWorker Worker);

    private readonly IServiceProvider _serviceProvider;
    private readonly BakhooOptions _options;
    private readonly ILogger _logger;

    private List<WorkerContext> _workerContexts = new();

    public BakhooJobManager(
        IServiceProvider serviceProvider,
        IOptions<BakhooOptions> options,
        ILogger<BakhooJobManager> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested || _workerContexts.Count > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            _logger.LogDebug("Starting another cycle...");

            lock (_workerContexts)
            {
                _logger.LogDebug("Checking that any workers have completed...");

                foreach (var workerContext in _workerContexts.ToArray())
                {
                    if (workerContext.Worker.RunTask == null
                        || (
                            workerContext.Worker.RunTask.IsCompleted
                            && (
                                workerContext.Worker.CancelationTask == null
                                || workerContext.Worker.CancelationTask.IsCompleted
                            )
                        ))
                    {
                        _logger.LogDebug("Found that the worker for {TaskId} has completed. Disposing the worker context.", workerContext.Worker.JobId);

                        workerContext.Scope.Dispose();
                        _workerContexts.Remove(workerContext);
                    }
                }

                if (_workerContexts.Count >= _options.MaxParallelTasks)
                    continue;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var importService = scope.ServiceProvider.GetRequiredService<IBakhooJobStateService>();
                var tasks = importService.GetJobsInBacklogAsync();

                await foreach (var importTask in tasks)
                {
                    if (ct.IsCancellationRequested)
                        break;
                    if (_workerContexts.Count >= _options.MaxParallelTasks)
                        break;

                    var workerScope = _serviceProvider.CreateScope();
                    var worker = workerScope.ServiceProvider.GetRequiredService<IBakhooWorker>();
                    await worker.StartWorkerAsync(importTask.Id, ct);

                    _workerContexts.Add(new WorkerContext(workerScope, worker));
                }

                if (ct.IsCancellationRequested)
                {
                    foreach (var workerContext in _workerContexts)
                    {
                        workerContext.Worker.Cancel();
                    }
                }

                tasks = importService.GetJobsToCancelAsync();
                await foreach (var importTask in tasks)
                {
                    if (_workerContexts.Any(x => x.Worker.JobId == importTask.Id))
                    {
                        var context = _workerContexts.First(x => x.Worker.JobId == importTask.Id);
                        context.Worker.Cancel();
                    }
                }
            }
        }
    }
}
