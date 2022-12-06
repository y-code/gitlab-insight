using Microsoft.Extensions.Options;
using YouTrackInsight.Entity;
using YouTrackInsight.Domain;
using Bakhoo;

namespace YouTrackInsight.Services;

public class YTIssueImportService : IBakhooJobHandler<IssueImportJob>
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

    public async Task Handle(IssueImportJob job, CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), ct);
    }
}
