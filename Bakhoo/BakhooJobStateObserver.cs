namespace Bakhoo;

public interface IBakhooJobStateObserver
{
    Task NotifyIssueImportJobUpdatedAsync(Guid jobId, CancellationToken ct);
}
