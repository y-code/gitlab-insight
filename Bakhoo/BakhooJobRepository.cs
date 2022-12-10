using System.Text.Json;
using System.Threading.Tasks;
using Bakhoo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bakhoo;

internal interface IBakhooJobRepository
{
    IAsyncEnumerable<BakhooJob> GetJobsAsync();
    IAsyncEnumerable<BakhooJob> GetJobsToCancelAsync();
    Task<BakhooJob> GetJobAsync(Guid id, CancellationToken ct);
    Task SubmitJobAsync<TData>(Guid id, TData jobData, CancellationToken ct);
    Task StartJobAsync(Guid id, CancellationToken ct);
    Task CancelJobAsync(Guid id, CancellationToken ct);
    Task UpdateSuccessfulJobStateAsync(Guid id, CancellationToken ct);
    Task UpdateCancelledJobAsync(Guid id, string message, CancellationToken ct);
    Task UpdateFailedJobStateAsync(Guid id, string message, CancellationToken ct);
}

internal class BakhooJobStateService : IBakhooJobRepository
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
        var oldestToDisplayCompletedJobs = DateTimeOffset.UtcNow - TimeSpan.FromHours(_options.MaxHoursToDisplayCompletedJobs);

        var jobsInBacklog = _db.Jobs
            .Where(x => !x.Start.HasValue)
            .OrderByDescending(x => x.Submitted);

        var jobsInProgress = _db.Jobs
            .Where(x => x.Start.HasValue && !x.End.HasValue)
            .OrderByDescending(x => x.Start);

        var jobsAlreadyDone = _db.Jobs
            .Where(x =>
                x.End.HasValue
                && x.End > oldestToDisplayCompletedJobs)
            .OrderByDescending(x => x.Start);

        return jobsInBacklog.Concat(jobsInProgress.Concat(jobsAlreadyDone))
            .AsAsyncEnumerable();
    }

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

        var existingIds = await _db.Jobs
            .Where(x => !x.Start.HasValue)
            .OrderBy(x => x.Submitted)
            .Select(x => x.Id)
            .ToAsyncEnumerable()
            .ToArrayAsync(ct);

        if (existingIds.Contains(id)) return;

        if (existingIds.Count() >= _options.MaxBacklogJobs)
            throw new InvalidOperationException($"{existingIds.Count()} jobs has already been waiting. It is not allowed to submit any more job.");

        await _db.AddAsync(new BakhooJob
        {
            Id = id,
            Type = typeof(TData).FullName,
            Submitted = DateTimeOffset.UtcNow,
            Message = "Waiting for the Issue Import job to begin.",
            Data = JsonSerializer.Serialize(jobData),
        }, ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task StartJobAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        var job = await GetJobAsync(id, ct);

        if (job == null)
            throw new ArgumentException($"There is no job with ID {id}", nameof(id));

        if (job.IsCancelled) return;

        job.Start = DateTimeOffset.UtcNow;
        job.Message = "Issue import is in progress.";

        if (job.IsCancelling)
        {
            job.IsCancelled = true;
            job.End = job.Start;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelJobAsync(Guid id, CancellationToken ct)
    {
        if (id == default)
            throw new ArgumentException($"A UUID is required for parameter `id` is required.", nameof(id));

        var job = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (job == null)
            throw new InvalidOperationException($"There is no job with ID {id}.");

        if (job.IsCancelled)
            throw new InvalidOperationException($"Job {id} has already been canceled.");

        if (job.HasError)
            throw new InvalidOperationException($"Job {id} has had an error. So, cancellation is an invalid operation.");

        job.IsCancelling = true;
        job.CancelRequested = DateTimeOffset.UtcNow;
        job.Message = "A user has requested to cancel this job. The job cancellation is in progress...";

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateSuccessfulJobStateAsync(Guid id, CancellationToken ct)
    {
        var job = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (job == null)
            throw new ArgumentException($"There is no job with ID {id}");

        job.End = DateTimeOffset.UtcNow;
        job.Message = "Issue import has been successfully completed.";

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateCancelledJobAsync(Guid id, string message, CancellationToken ct)
    {
        var job = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (job == null)
            throw new ArgumentException($"There is no job with ID {id}");

        job.IsCancelling = false;
        job.IsCancelled = true;
        job.Start ??= DateTimeOffset.UtcNow;
        job.End = DateTimeOffset.UtcNow;
        job.Message = message;

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateFailedJobStateAsync(Guid id, string message, CancellationToken ct)
    {
        var job = (await _db.Jobs
            .Where(x => x.Id == id)
            .ToAsyncEnumerable()
            .ToListAsync(ct))
            .FirstOrDefault();

        if (job == null)
            throw new ArgumentException($"There is no job with ID {id}");

        job.IsCancelling = false;
        job.IsCancelled = false;
        job.End = DateTimeOffset.UtcNow;
        job.HasError = true;
        job.Message = message;

        await _db.SaveChangesAsync(ct);
    }
}
