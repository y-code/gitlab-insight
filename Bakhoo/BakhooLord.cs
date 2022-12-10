using System.Threading.Tasks;
using Bakhoo.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bakhoo;

internal class BakhooLord : BackgroundService
{
    private record WorkerContext(
        IServiceScope Scope,
        IBakhooWorker Worker);

    private readonly IServiceProvider _serviceProvider;
    private readonly BakhooOptions _options;
    private readonly IBakhooJobSequencer _jobSequencer;
    private readonly ILogger _logger;

    private List<WorkerContext> _workerContexts = new();
    private bool _isStopping = false;

    public BakhooLord(
        IServiceProvider serviceProvider,
        IOptions<BakhooOptions> options,
        IBakhooJobSequencer jobSequencer,
        ILogger<BakhooLord> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _jobSequencer = jobSequencer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
        _logger.LogInformation("Max Worker Threads: {MaxWorkerThreads}", maxWorkerThreads);
        _logger.LogInformation("Max Completion Port Threads: {MaxCompletionPortThreads}", maxCompletionPortThreads);

        while (!ct.IsCancellationRequested || _workerContexts.Count > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            _logger.LogDebug("Starting another cycle...");
            _logger.LogInformation("Worker Count: {WorkerCount}", _workerContexts.Count());

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
                        _logger.LogDebug("Found that the worker for {JobId} has completed. Disposing the worker context.", workerContext.Worker.JobId);

                        workerContext.Scope.Dispose();
                        _workerContexts.Remove(workerContext);
                    }
                }
            }

            if (_workerContexts.Count < _options.MaxParallelJobs)
            {
                var jobIds = _jobSequencer.GetJobsInBacklogAsync();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IBakhooJobRepository>();
                    var cancellingJobs = new List<BakhooJob>();

                    await foreach (var jobId in jobIds)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        var job = await jobRepo.GetJobAsync(jobId, ct);
                        if (job.IsCancelling)
                        {
                            cancellingJobs.Add(job);
                            continue;
                        }

                        if (_workerContexts.Count >= _options.MaxParallelJobs)
                            break;

                        var workerScope = _serviceProvider.CreateScope();
                        var worker = workerScope.ServiceProvider.GetRequiredService<IBakhooWorker>();
                        await worker.StartWorkerAsync(job.Id, ct);

                        _workerContexts.Add(new WorkerContext(workerScope, worker));
                    }

                    foreach (var job in cancellingJobs)
                    {
                        var monitor = scope.ServiceProvider.GetRequiredService<IBakhooJobMonitor>();
                        await jobRepo.UpdateCancelledJobAsync(job.Id, "The job was canceled.", ct);
                        await monitor.NotifyIssueImportJobUpdatedAsync(job.Id, ct);
                    }
                }
            }

            if (ct.IsCancellationRequested && !_isStopping)
            {
                _isStopping = true;

                _logger.LogInformation("Job manager is requested to stop, and stopping...");

                foreach (var workerContext in _workerContexts)
                {
                    workerContext.Worker.Cancel();
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                _logger.LogDebug("Checking jobs to cancell...");

                var importService = scope.ServiceProvider.GetRequiredService<IBakhooJobRepository>();

                var jobs = importService.GetJobsToCancelAsync();
                var jobCancellationCount = 0;
                var startedJobCancellationCount = 0;
                await foreach (var job in jobs)
                {
                    jobCancellationCount++;

                    if (_workerContexts.Any(x => x.Worker.JobId == job.Id))
                    {
                        startedJobCancellationCount++;

                        var context = _workerContexts.First(x => x.Worker.JobId == job.Id);
                        context.Worker.Cancel();
                    }
                }

                _logger.LogInformation("Job Cancellation Count: {JobCancellationCount}", jobCancellationCount);
                _logger.LogInformation("Started Job Cancellation Count: {StartedJobCancellationCount}", startedJobCancellationCount);
            }
        }
    }
}
