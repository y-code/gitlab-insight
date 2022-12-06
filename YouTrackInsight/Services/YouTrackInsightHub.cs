using Bakhoo;
using Microsoft.AspNetCore.SignalR;

namespace YouTrackInsight.Services;

public class YouTrackInsightHub : Hub
{
    public const string GROUP_ISSUE_IMPORT = "Issue Import";

    public YouTrackInsightHub()
    {
    }

    public async Task SubscribeToIssueImport()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GROUP_ISSUE_IMPORT);
    }

    public async Task UnsubscribeToIssueImport()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GROUP_ISSUE_IMPORT);
    }
}

public class YouTrackInsightHubClients : IBakhooJobStateObserver
{
    const string OnIssueImportTaskUpdated = nameof(OnIssueImportTaskUpdated);

    private readonly IHubContext<YouTrackInsightHub> _context;

    public YouTrackInsightHubClients(IHubContext<YouTrackInsightHub> context)
    {
        _context = context;
    }

    public async Task NotifyIssueImportJobUpdatedAsync(Guid taskId, CancellationToken ct)
    {
        await _context.Clients.Group(YouTrackInsightHub.GROUP_ISSUE_IMPORT)
            .SendAsync(OnIssueImportTaskUpdated, taskId, ct);
    }
}
