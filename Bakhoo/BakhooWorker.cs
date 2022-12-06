using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Bakhoo.Entity;
using Microsoft.Extensions.Logging;

namespace Bakhoo;

internal interface IBakhooWorker
{
    Guid JobId { get; }
    Task? RunTask { get; }
    Task? CancelationTask { get; }
    Task StartWorkerAsync(Guid jobId, CancellationToken ct);
    void Cancel();
}

internal class BakhooWorker : IBakhooWorker
{
    private readonly IBakhooJobStateService _importService;
    private readonly IBakhooJobStateObserver _observer;
    private readonly IEnumerable<IBakhooJobHandler> _jobHandlers;
    private readonly ILogger _logger;

    public Guid JobId { get; private set; }
    public Task? RunTask { get; private set; }
    private CancellationTokenSource? _taskCts;
    public Task? CancelationTask { get; private set; }

    public BakhooWorker(
        IBakhooJobStateService importService,
        IBakhooJobStateObserver observer,
        IEnumerable<IBakhooJobHandler> jobHandlers,
        ILogger<BakhooWorker> logger)
    {
        _importService = importService;
        _observer = observer;
        _jobHandlers = jobHandlers;
        _logger = logger;
    }

    public async Task StartWorkerAsync(Guid jobId, CancellationToken ct)
    {
        JobId = jobId;
        try
        {
            await _importService.StartJobAsync(jobId, ct);
            await _observer.NotifyIssueImportJobUpdatedAsync(jobId, ct);

            _taskCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            RunTask = RunAsync(_taskCts.Token);
        }
        catch (Exception e)
        {
            await _importService.UpdateFailedJobStateAsync(jobId, $"Issue import failed. {e.Message}", ct);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            var job = await _importService.GetJobAsync(JobId, ct);
            if (job.Type == null) throw new ArgumentException($"Job {JobId} does not have job type.");
            if (job.Data == null) throw new ArgumentException($"Job {JobId} does not have job data.");
            var jobData = job.Data;

            var handlers = _jobHandlers
                .Select(handler =>
                    (
                        handler,
                        jobType: handler.GetType().GetInterfaces()
                            .FirstOrDefault(x
                                => x.IsGenericType
                                    && x.Name == typeof(IBakhooJobHandler<>).Name)
                                ?.GenericTypeArguments.First()
                    )
                )
                .Where(x => x.jobType?.FullName == job.Type)
                .ToArray();

            if (!handlers.Any()) throw new InvalidOperationException($"There is no job handler for job type {job.Type}");

            var jobType = handlers.First().jobType;
            if (jobType == null) throw new ArgumentException($"Job {JobId} has an invalid job type.");

            object? data = null;
            try
            {
                data = JsonSerializer.Deserialize(jobData, jobType);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed parsing job data of job {JobId}.", e);
            }
            if (data == null) throw new ArgumentException($"Failed parsing job data of job {JobId}.");

            var handleTasks = new List<Task>();
            foreach (var (handler, _) in handlers)
            {
                if (jobType == null) continue;
                handleTasks.Add(HandleJob(data, handler, jobType, ct));
            }
            await Task.WhenAll(handleTasks.ToArray());
        }
        catch (Exception e)
        {
            await _importService.UpdateFailedJobStateAsync(JobId, e.Message, ct);
            throw;
        }

        await _importService.UpdateSuccessfulJobStateAsync(JobId, ct);

        await _observer.NotifyIssueImportJobUpdatedAsync(JobId, ct);
    }

    private Task HandleJob(object data, IBakhooJobHandler handler, Type jobType, CancellationToken ct)
    {
        var handle = typeof(IBakhooJobHandler<>).MakeGenericType(jobType)
            .GetMethod(GetMethodName<IBakhooJobHandler<object>>(x => x.Handle(ItIsAnyObject(), ItIsCT())));
        if (handle == null || handle.ReturnType != typeof(Task))
            throw new InvalidOperationException($"Something wrong in Bakhoo implementation.");

        var handleTask = (Task?)handle.Invoke(handler, new object?[] { data, ct });
        if (handleTask == null)
            throw new InvalidOperationException($"Something wrong in Bakhoo implementation.");
        return handleTask;
    }

    private static string GetMethodName<T>(Expression<Action<T>> expression)
        => ((MethodCallExpression)expression.Body).Method.Name;

    private static object ItIsAnyObject() => new object();
    private static CancellationToken ItIsCT() => new CancellationTokenSource().Token;

    private class ExampleJobHandler : IBakhooJobHandler<object>
    {
        public Task Handle(object job, CancellationToken ct) => Task.CompletedTask;
    }

    public void Cancel()
    {
        if (RunTask == null || _taskCts == null || CancelationTask != null) return;

        _taskCts.Cancel();
        var cancelCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        CancelationTask = InnerCancelAsync(RunTask, _taskCts, cancelCts.Token);
    }

    private async Task InnerCancelAsync(Task runTask, CancellationTokenSource taskCts, CancellationToken cancelCt)
    {
        try
        {
            var isInTime = false;
            try
            {
                isInTime = runTask.Wait(TimeSpan.FromSeconds(20));
            }
            catch (TaskCanceledException e)
            {
                await UpdateCancelledJobStateAsync(cancelCt);
            }
            catch (AggregateException e)
            {
                var taskCanceledException = e.InnerExceptions
                    .FirstOrDefault(x => x is TaskCanceledException);
                if (taskCanceledException == null)
                    await _importService.UpdateFailedJobStateAsync(JobId, e.Message, cancelCt);

                await UpdateCancelledJobStateAsync(cancelCt);
            }

            if (isInTime)
            {
                if (taskCts.IsCancellationRequested)
                    await UpdateCancelledJobStateAsync(cancelCt);
                else
                    await _importService.UpdateSuccessfulJobStateAsync(JobId, cancelCt);
            }
            else
                await _importService.UpdateFailedJobStateAsync(JobId, "The cancellation timed out.", cancelCt);

            await _observer.NotifyIssueImportJobUpdatedAsync(JobId, cancelCt);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private Task UpdateCancelledJobStateAsync(CancellationToken ct)
        => _importService.UpdateCancelledJobStateAsync(JobId, "The task was canceled", ct);
}
