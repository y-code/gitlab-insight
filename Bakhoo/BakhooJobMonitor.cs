namespace Bakhoo;

public interface IBakhooJobMonitor
{
    Task NotifyIssueImportJobUpdatedAsync(Guid jobId, CancellationToken ct);
}
