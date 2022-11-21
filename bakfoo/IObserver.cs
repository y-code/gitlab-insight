namespace Bakfoo;

public interface IBakfooObserver
{
    Task NotifyIssueImportTaskUpdatedAsync(Guid taskId, CancellationToken ct);
}
