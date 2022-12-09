using System;
using Bakhoo.Entity;

namespace Bakhoo;

public interface IBakhooJobWindow
{
    IAsyncEnumerable<BakhooJob> GetJobsAsync();
    Task SubmitJobAsync<TData>(Guid id, TData jobData, CancellationToken ct);
    Task CancelJobAsync(Guid id, CancellationToken ct);
}

internal class BakhooJobWindow : IBakhooJobWindow
{
    private readonly IBakhooJobRepository _jobRepo;

    public BakhooJobWindow(IBakhooJobRepository jobRepo)
	{
        _jobRepo = jobRepo;
	}

    public Task CancelJobAsync(Guid id, CancellationToken ct)
        => _jobRepo.CancelJobAsync(id, ct);

    public IAsyncEnumerable<BakhooJob> GetJobsAsync()
        => _jobRepo.GetJobsAsync();

    public Task SubmitJobAsync<TData>(Guid id, TData jobData, CancellationToken ct)
        => _jobRepo.SubmitJobAsync(id, jobData, ct);
}
