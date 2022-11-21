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
}
