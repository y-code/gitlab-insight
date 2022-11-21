using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Bakfoo;

public class BakfooJobManager : BackgroundService
{
    private record WorkerContext(
        IServiceScope Scope,
        BakfooWorker Worker);

    private readonly IServiceProvider _serviceProvider;
    private readonly BakfooOptions _options;

    private List<WorkerContext> _workerContexts = new();

    public BakfooJobManager(
        IServiceProvider serviceProvider,
        IOptions<BakfooOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            lock (_workerContexts)
            {
                foreach (var workerContext in _workerContexts.ToArray())
                {
                    if (workerContext.Worker.RunTask == null
                        || (
                            workerContext.Worker.RunTask.IsCompleted
                            && (
                                workerContext.Worker.CancelTask == null
                                || workerContext.Worker.CancelTask.IsCompleted
                            )
                        ))
                    {
                        workerContext.Scope.Dispose();
                        _workerContexts.Remove(workerContext);
                    }
                }

                if (_workerContexts.Count >= _options.MaxParallelTasks)
                    continue;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var importService = scope.ServiceProvider.GetRequiredService<BakfooService>();
                var tasks = importService.GetTasksInBacklogAsync();

                await foreach (var importTask in tasks)
                {
                    if (ct.IsCancellationRequested)
                        return;
                    if (_workerContexts.Count >= _options.MaxParallelTasks)
                        break;

                    var workerScope = _serviceProvider.CreateScope();
                    var worker = workerScope.ServiceProvider.GetRequiredService<BakfooWorker>();
                    await worker.StartWorkerAsync(importTask.Id, ct);

                    _workerContexts.Add(new WorkerContext(workerScope, worker));
                }

                tasks = importService.GetTasksToCancelAsync();
                await foreach (var importTask in tasks)
                {
                    if (_workerContexts.Any(x => x.Worker.TaskId == importTask.Id))
                    {
                        var context = _workerContexts.First(x => x.Worker.TaskId == importTask.Id);
                        context.Worker.Cancel();
                    }
                }
            }
        }
    }
}
