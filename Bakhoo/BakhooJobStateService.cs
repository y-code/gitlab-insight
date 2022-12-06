using System.Text.Json;
using System.Threading.Tasks;
using Bakhoo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bakhoo;

public interface IBakhooJobStateService
{
    IAsyncEnumerable<BakhooJob> GetJobsAsync();
    IAsyncEnumerable<BakhooJob> GetJobsInBacklogAsync();
    IAsyncEnumerable<BakhooJob> GetJobsToCancelAsync();
    Task<BakhooJob> GetJobAsync(Guid id, CancellationToken ct);
    Task SubmitJobAsync<TData>(Guid id, TData jobData, CancellationToken ct);
    Task StartJobAsync(Guid id, CancellationToken ct);
    Task CancelJobAsync(Guid id, CancellationToken ct);
    Task UpdateSuccessfulJobStateAsync(Guid id, CancellationToken ct);
    Task UpdateCancelledJobStateAsync(Guid taskId, string message, CancellationToken ct);
    Task UpdateFailedJobStateAsync(Guid taskId, string message, CancellationToken ct);
}

internal class BakhooJobStateService : IBakhooJobStateService
{
    private readonly BakhooDbContext _db;
    private readonly BakhooOptions _options;

    public BakhooJobStateService(
        BakhooDbContext db,
        IOptions<BakhooOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public IAsyncEnumerable<BakhooJob> GetJobsAsync()
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

    public IQueryable<BakhooJob> TasksInBacklog
        => _db.Jobs
            .Where(x => !x.Start.HasValue)
            .OrderBy(x => x.Submitted);

    public IAsyncEnumerable<BakhooJob> GetJobsInBacklogAsync()
        => TasksInBacklog.ToAsyncEnumerable();

    public IAsyncEnumerable<BakhooJob> GetJobsToCancelAsync()
        => _db.Jobs
            .Where(x => x.IsCancelling && !x.End.HasValue)
            .ToAsyncEnumerable();

    public async Task<BakhooJob> GetJobAsync(Guid id, CancellationToken ct)
        => (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault() ?? throw new ArgumentException($"There is no job with ID {id}");

    public async Task SubmitJobAsync<TData>(Guid id, TData jobData, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        var existingIds = await TasksInBacklog.Select(x => x.Id)
            .ToAsyncEnumerable().ToArrayAsync(ct);

        if (existingIds.Contains(id)) return;

        if (existingIds.Count() >= _options.MaxBacklogTasks)
            throw new InvalidOperationException($"{existingIds.Count()} tasks has already been waiting. It is not allowed to submit any more task.");

        await _db.AddAsync(new BakhooJob
        {
            Id = id,
            Type = typeof(TData).FullName,
            Submitted = DateTimeOffset.UtcNow,
            Message = "Waiting for the Issue Import task to begin.",
            Data = JsonSerializer.Serialize(jobData),
        }, ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task StartJobAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        var importTask = await GetJobAsync(id, ct);

        if (importTask == null)
            throw new ArgumentException($"There is no task with ID {id}", nameof(id));

        if (importTask.IsCancelled) return;

        importTask.Start = DateTimeOffset.UtcNow;
        importTask.Message = "Issue import is in progress.";

        if (importTask.IsCancelling)
        {
            importTask.IsCancelled = true;
            importTask.End = importTask.Start;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelJobAsync(Guid id, CancellationToken ct)
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

    public async Task UpdateSuccessfulJobStateAsync(Guid id, CancellationToken ct)
    {
        var task = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (task == null)
            throw new ArgumentException($"There is no task with ID {id}");

        task.End = DateTimeOffset.UtcNow;
        task.Message = "Issue import has been successfully completed.";

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateCancelledJobStateAsync(Guid id, string message, CancellationToken ct)
    {
        var task = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (task == null)
            throw new ArgumentException($"There is no task with ID {id}");

        task.IsCancelling = false;
        task.IsCancelled = true;
        task.End = DateTimeOffset.UtcNow;
        task.Message = message;

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateFailedJobStateAsync(Guid id, string message, CancellationToken ct)
    {
        var task = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (task == null)
            throw new ArgumentException($"There is no task with ID {id}");

        task.IsCancelling = false;
        task.IsCancelled = false;
        task.End = DateTimeOffset.UtcNow;
        task.HasError = true;
        task.Message = message;

        await _db.SaveChangesAsync(ct);
    }
}
