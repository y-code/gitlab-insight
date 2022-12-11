using Microsoft.Extensions.Options;
using YouTrackInsight.Entity;
using YouTrackInsight.Domain;
using Bakhoo;
using Microsoft.EntityFrameworkCore;

namespace YouTrackInsight.Services;

public class YTIssueImportService : IBakhooJobHandler<YTIssueImportJob>
{
    private readonly ILogger _logger;
    private readonly YTInsightDbContext _db;
    private readonly IYouTrackClientService _client;
    private readonly YouTrackInsightOptions _options;

    public YTIssueImportService(
        YTInsightDbContext db,
        IYouTrackClientService client,
        IOptions<YouTrackInsightOptions> options,
        ILogger<YTIssueImportService> logger)
    {
        _db = db;
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Handle(Guid jobId, YTIssueImportJob job, CancellationToken ct)
    {
        _logger.LogInformation("Issue Import {JobId} started.", jobId);

        var options = new YTSearchOptions();
        var issues = _client.GetIssuesAsync(options);

        var count = 0;
        await foreach (var issue in issues)
        {
            count++;

            var existing = await _db.Issues.Where(x => x.Id == issue.Id)
                .Include(x => x.Links)
                .Select(x => new { x.Id, x.Version, x.Links })
                .ToAsyncEnumerable()
                .ToArrayAsync();

            if (existing.Length == 0)
            {
                _logger.LogDebug("Issue Import {JobId}: Adding Issue {IssueId}... ({Count})", jobId, issue.Id, count);

                await _db.Issues.AddAsync(issue);
            }
            else
            {
                _logger.LogDebug("Issue Import {JobId}: Updating Issue {IssueId}... ({Count})", jobId, issue.Id, count);

                _db.IssueLinks.RemoveRange(existing[0].Links);

                issue.Version = existing[0].Version;
                _db.Issues.Update(issue);
            }
        }

        _logger.LogDebug("Issue Import {JobId}: Commiting {Count} issues...", jobId, count);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Issue Import {JobId} completed.", jobId);
    }
}
