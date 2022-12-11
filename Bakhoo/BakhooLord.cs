using System.Threading.Tasks;
using Bakhoo.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bakhoo;

internal class BakhooLord : BackgroundService
{
    private record VassalContext(
        IServiceScope Scope,
        IBakhooVassal Vassal);

    private readonly IServiceProvider _serviceProvider;
    private readonly BakhooOptions _options;
    private readonly IBakhooJobSequencer _jobSequencer;
    private readonly ILogger _logger;

    private List<VassalContext> _vassalContexts = new();
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
        ThreadPool.GetMaxThreads(out var maxVassalThreads, out var maxCompletionPortThreads);
        _logger.LogInformation("Max Vassal Threads: {MaxVassalThreads}", maxVassalThreads);
        _logger.LogInformation("Max Completion Port Threads: {MaxCompletionPortThreads}", maxCompletionPortThreads);

        while (!ct.IsCancellationRequested || _vassalContexts.Count > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            _logger.LogDebug("Starting another cycle...");
            _logger.LogInformation("Vassal Count: {VassalCount}", _vassalContexts.Count());

            lock (_vassalContexts)
            {
                _logger.LogDebug("Checking that any vassals have completed...");

                foreach (var vassalContext in _vassalContexts.ToArray())
                {
                    if (vassalContext.Vassal.RunTask == null
                        || (
                            vassalContext.Vassal.RunTask.IsCompleted
                            && (
                                vassalContext.Vassal.CancelationTask == null
                                || vassalContext.Vassal.CancelationTask.IsCompleted
                            )
                        ))
                    {
                        _logger.LogDebug("Found that the vassal for {JobId} has completed. Disposing the vassal context.", vassalContext.Vassal.JobId);

                        vassalContext.Scope.Dispose();
                        _vassalContexts.Remove(vassalContext);
                    }
                }
            }

            if (_vassalContexts.Count < _options.MaxParallelJobs && !_isStopping)
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

                        if (_vassalContexts.Count >= _options.MaxParallelJobs)
                            break;

                        var vassalScope = _serviceProvider.CreateScope();
                        var vassal = vassalScope.ServiceProvider.GetRequiredService<IBakhooVassal>();
                        await vassal.StartAsync(job.Id, ct);

                        _vassalContexts.Add(new VassalContext(vassalScope, vassal));
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

                foreach (var vassalContext in _vassalContexts)
                {
                    vassalContext.Vassal.Cancel();
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

                    if (_vassalContexts.Any(x => x.Vassal.JobId == job.Id))
                    {
                        startedJobCancellationCount++;

                        var context = _vassalContexts.First(x => x.Vassal.JobId == job.Id);
                        context.Vassal.Cancel();
                    }
                }

                _logger.LogInformation("Job Cancellation Count: {JobCancellationCount}", jobCancellationCount);
                _logger.LogInformation("Started Job Cancellation Count: {StartedJobCancellationCount}", startedJobCancellationCount);
            }
        }
    }
}
