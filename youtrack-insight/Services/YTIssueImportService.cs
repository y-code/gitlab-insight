using Microsoft.Extensions.Options;
using YouTrackInsight.Entity;
using YouTrackInsight.Domain;

namespace YouTrackInsight.Services;

public class YTIssueImportService
{
    private readonly YTInsightDbContext _db;
    private readonly YouTrackInsightOptions _options;

    public YTIssueImportService(
        YTInsightDbContext db,
        IOptions<YouTrackInsightOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public IAsyncEnumerable<YTIssueImportTask> GetTasksAsync()
        => _db.IssueImportTasks
            .OrderByDescending(x => x.Submitted)
            .Take(_options.IssueImport.MaxBacklogTasks + _options.IssueImport.MaxParallelTasks)
            .ToAsyncEnumerable();

    public IQueryable<YTIssueImportTask> TasksInBacklog
        => _db.IssueImportTasks
            .Where(x => !x.Start.HasValue)
            .OrderBy(x => x.Submitted);

    public IAsyncEnumerable<YTIssueImportTask> GetTasksInBacklogAsync()
        => TasksInBacklog.ToAsyncEnumerable();

    public async Task<YTIssueImportTask?> GetTaskAsync(Guid taskId, CancellationToken ct)
        => (await _db.IssueImportTasks
            .Where(x => x.Id == taskId)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

    public async Task SubmitTaskAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var existingIds = await TasksInBacklog.Select(x => x.Id)
            .ToAsyncEnumerable().ToArrayAsync(ct);

        if (existingIds.Contains(id)) return;

        if (existingIds.Count() >= _options.IssueImport.MaxBacklogTasks)
            throw new InvalidOperationException($"{existingIds.Count()} tasks has already been waiting. It is not allowed to submit any more task.");

        await _db.AddAsync(new YTIssueImportTask
        {
            Id = id,
            Submitted = DateTimeOffset.UtcNow,
            Message = "Waiting for the Issue Import task to begin.",
        }, ct);

        await _db.SaveChangesAsync(ct);

        await _db.Database.CommitTransactionAsync(ct);
    }

    public async Task CancelTaskAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var task = (await _db.IssueImportTasks
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (task == null)
            throw new InvalidOperationException($"There is no task with ID {id}.");

        if (task.IsCancelled)
            throw new InvalidOperationException($"Task {id} has already been cancelled.");

        if (task.HasError)
            throw new InvalidOperationException($"Task {id} has had an error. So, cancellation is an invalid operation.");

        task.IsCancelled = true;
        task.End = DateTimeOffset.UtcNow;
        task.Start ??= task.End;
        task.Message = "The task is cancelled by a user request.";

        await _db.SaveChangesAsync(ct);

        await _db.Database.CommitTransactionAsync(ct);
    }

    public async Task UpdateSuccessfulTaskStateAsync(Guid taskId, CancellationToken ct)
    {
        using (var tx = await _db.Database.BeginTransactionAsync())
        {
            var task = (await _db.IssueImportTasks
                .Where(x => x.Id == taskId)
                .ToAsyncEnumerable()
                .ToListAsync(ct))
                .FirstOrDefault();

            if (task == null)
                throw new ArgumentException($"There is no task with ID {taskId}");

            task.End = DateTimeOffset.UtcNow;
            task.Message = "Issue import has been successfully completed.";

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync();
        }
    }

    public async Task UpdateFailedTaskStateAsync(Guid taskId, string message, CancellationToken ct)
    {
        using (var tx = await _db.Database.BeginTransactionAsync())
        {
            var task = (await _db.IssueImportTasks
                .Where(x => x.Id == taskId)
                .ToAsyncEnumerable()
                .ToListAsync(ct))
                .FirstOrDefault();

            if (task == null)
                throw new ArgumentException($"There is no task with ID {taskId}");

            task.End = DateTimeOffset.UtcNow;
            task.HasError = true;
            task.Message = $"Issue import failed. {message}";

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync();
        }
    }
}
