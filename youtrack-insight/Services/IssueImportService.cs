using System;
using Microsoft.Extensions.Options;
using YouTrackInsight.Entity;
using YouTrackInsight.Domain;
using System.Net;

namespace YouTrackInsight.Services;

public class YTIssueImportService
{
    private readonly YTInsightDbContext _db;
    private readonly YouTrackInsightOptions _options;

    public YTIssueImportService(YTInsightDbContext db, IOptions<YouTrackInsightOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public IAsyncEnumerable<YTIssueImportTask> GetTasksInProgress()
        => _db.IssueImportTasks
            .Where(x => !x.End.HasValue)
            .ToAsyncEnumerable();

    public async Task SubmitTaskAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var existingIds = await _db.IssueImportTasks
            .Where(x => !x.End.HasValue)
            .Select(x => x.Id)
            .ToAsyncEnumerable().ToArrayAsync(ct);

        if (existingIds.Contains(id)) return;

        if (existingIds.Count() >= _options.IssueImport.MaxParallelTasks)
            throw new InvalidOperationException($"{existingIds.Count()} tasks has already been in progress. It is not allowed to submit any more task.");

        await _db.AddAsync(new YTIssueImportTask
        {
            Id = id,
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
        task.Message = "The task is cancelled by a user request.";

        await _db.SaveChangesAsync(ct);

        await _db.Database.CommitTransactionAsync(ct);
    }
}
