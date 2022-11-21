using System.Threading.Tasks;
using Bakfoo.Entity;
using Microsoft.Extensions.Logging;

namespace Bakfoo
{
    public class BakfooWorker
    {
        private readonly BakfooDbContext _db;
        private readonly BakfooService _importService;
        private readonly IBakfooObserver _observer;
        private readonly ILogger _logger;

        public Guid TaskId { get; set; }
        public Task? RunTask { get; private set; }
        private CancellationTokenSource? _taskCts;
        public Task? CancelTask { get; private set; }

        public BakfooWorker(
            BakfooDbContext db,
            BakfooService importService,
            IBakfooObserver observer,
            ILogger<BakfooWorker> logger)
        {
            _db = db;
            _importService = importService;
            _observer = observer;
            _logger = logger;
        }

        public async Task StartWorkerAsync(Guid taskId, CancellationToken ct)
        {
            TaskId = taskId;
            try
            {
                var importTask = await _importService.GetTaskAsync(taskId, ct);

                if (importTask == null)
                    throw new ArgumentException($"There is no task with ID {taskId}");

                importTask.Start = DateTimeOffset.UtcNow;
                importTask.Message = "Issue import is in progress.";

                if (importTask.IsCancelling)
                {
                    importTask.IsCancelled = true;
                    importTask.End = importTask.Start;
                }

                await _db.SaveChangesAsync(ct);

                if (importTask.IsCancelled) return;

                await _observer.NotifyIssueImportTaskUpdatedAsync(taskId, ct);

                _taskCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                RunTask = InnerRunAsync(_taskCts.Token);
            }
            catch (Exception e)
            {
                await _importService.UpdateFailedTaskStateAsync(taskId, $"Issue import failed. {e.Message}", ct);
            }
        }

        private async Task InnerRunAsync(CancellationToken ct)
        {
            await RunAsync(TaskId, ct);

            await _importService.UpdateSuccessfulTaskStateAsync(TaskId, ct);

            await _observer.NotifyIssueImportTaskUpdatedAsync(TaskId, ct);
        }

        public void Cancel()
        {
            if (RunTask == null || _taskCts == null || CancelTask != null) return;

            _taskCts.Cancel();
            var cancelCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            CancelTask = InnerCancelAsync(RunTask, _taskCts, cancelCts.Token);
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
                    await UpdateCanceledTaskStateAsync(cancelCt);
                }
                catch (AggregateException e)
                {
                    var taskCanceledException = e.InnerExceptions
                        .FirstOrDefault(x => x is TaskCanceledException);
                    if (taskCanceledException == null)
                        await _importService.UpdateFailedTaskStateAsync(TaskId, e.Message, cancelCt);

                    await UpdateCanceledTaskStateAsync(cancelCt);
                }

                if (isInTime)
                {
                    if (taskCts.IsCancellationRequested)
                        await UpdateCanceledTaskStateAsync(cancelCt);
                    else
                    {
                        await _importService.UpdateSuccessfulTaskStateAsync(TaskId, cancelCt);
                        await _observer.NotifyIssueImportTaskUpdatedAsync(TaskId, cancelCt);
                    }
                }
                else
                    await _importService.UpdateFailedTaskStateAsync(TaskId, "The cancellation timed out.", cancelCt);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private async  Task UpdateCanceledTaskStateAsync(CancellationToken ct)
        {
            await _importService.UpdateFailedTaskStateAsync(TaskId, "The task was canceled", ct);
            await _observer.NotifyIssueImportTaskUpdatedAsync(TaskId, ct);
        }

        public async Task RunAsync(Guid taskId, CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
        }
    }
}
