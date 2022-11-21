using Bakfoo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bakfoo;

public class BakfooService
{
    private readonly BakfooDbContext _db;
    private readonly BakfooOptions _options;

    public BakfooService(
        BakfooDbContext db,
        IOptions<BakfooOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public IAsyncEnumerable<BakfooJob> GetTasksAsync()
    {
        var oldestToDisplayCompletedTasks = DateTimeOffset.UtcNow - TimeSpan.FromHours(_options.MaxHoursToDisplayCompletedTasks);

        var tasksInBacklog = _db.Jobs
            .Where(x => !x.Start.HasValue)
            .OrderByDescending(x => x.Submitted);

        var tasksInProgress = _db.Jobs
            .Where(x => x.Start.HasValue && !x.End.HasValue)
            .OrderByDescending(x => x.Start);

        var tasksAlreadyDone = _db.Jobs
            .Where(x =>
                x.End.HasValue
                && x.End > oldestToDisplayCompletedTasks)
            .OrderByDescending(x => x.Start);

        return tasksInBacklog.Concat(tasksInProgress.Concat(tasksAlreadyDone))
            .AsAsyncEnumerable();
    }

    public IQueryable<BakfooJob> TasksInBacklog
        => _db.Jobs
            .Where(x => !x.Start.HasValue)
            .OrderBy(x => x.Submitted);

    public IAsyncEnumerable<BakfooJob> GetTasksInBacklogAsync()
        => TasksInBacklog.ToAsyncEnumerable();

    public IAsyncEnumerable<BakfooJob> GetTasksToCancelAsync()
        => _db.Jobs
            .Where(x => x.IsCancelling && !x.End.HasValue)
            .ToAsyncEnumerable();

    public async Task<BakfooJob?> GetTaskAsync(Guid taskId, CancellationToken ct)
        => (await _db.Jobs
            .Where(x => x.Id == taskId)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

    public async Task SubmitTaskAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        var existingIds = await TasksInBacklog.Select(x => x.Id)
            .ToAsyncEnumerable().ToArrayAsync(ct);

        if (existingIds.Contains(id)) return;

        if (existingIds.Count() >= _options.MaxBacklogTasks)
            throw new InvalidOperationException($"{existingIds.Count()} tasks has already been waiting. It is not allowed to submit any more task.");

        await _db.AddAsync(new BakfooJob
        {
            Id = id,
            Submitted = DateTimeOffset.UtcNow,
            Message = "Waiting for the Issue Import task to begin.",
        }, ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelTaskAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        var task = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (task == null)
            throw new InvalidOperationException($"There is no task with ID {id}.");

        if (task.IsCancelled)
            throw new InvalidOperationException($"Task {id} has already been canceled.");

        if (task.HasError)
            throw new InvalidOperationException($"Task {id} has had an error. So, cancellation is an invalid operation.");

        task.IsCancelling = true;
        task.CancelRequested = DateTimeOffset.UtcNow;
        task.Message = "A user has requested to cancel this task. The task cancellation is in progress...";

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateSuccessfulTaskStateAsync(Guid taskId, CancellationToken ct)
    {
        var task = (await _db.Jobs
            .Where(x => x.Id == taskId)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (task == null)
            throw new ArgumentException($"There is no task with ID {taskId}");

        task.IsCancelling = false;
        task.IsCancelled = true;
        task.End = DateTimeOffset.UtcNow;
        task.Message = "Issue import has been successfully completed.";

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateFailedTaskStateAsync(Guid taskId, string message, CancellationToken ct)
    {
        var task = (await _db.Jobs
            .Where(x => x.Id == taskId)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (task == null)
            throw new ArgumentException($"There is no task with ID {taskId}");

        task.IsCancelling = false;
        task.IsCancelled = false;
        task.End = DateTimeOffset.UtcNow;
        task.HasError = true;
        task.Message = message;

        await _db.SaveChangesAsync(ct);
    }
}
