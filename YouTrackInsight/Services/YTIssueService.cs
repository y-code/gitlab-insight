using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YouTrackInsight.Domain;
using YouTrackInsight.Entity;

namespace YouTrackInsight.Services;

public class YTIssueService
{
    private readonly ILogger _logger;
    private readonly YouTrackInsightOptions _options;
    private readonly YTInsightDbContext _db;

    public YTIssueService(
        IOptions<YouTrackInsightOptions> options,
        YTInsightDbContext db,
        ILogger<YTIssueService> logger)
	{
        _options = options.Value;
        _db = db;
        _logger = logger;
    }

    private static readonly string[] AggregationLinkTypes = new[]
    {
        "Subtask",
        "Depend",
    };

    public async Task<YTIssueNetworkModel> GetIssueNetwork(YTSearchOptions options)
    {
        var network = new YTIssueNetworkModel
        {
            Options = options,
            Issues = await _db.Issues
                .Include(x => x.Links)
                .Where(x => !options.Projects.Any() || options.Projects.Contains(x.ProjectId))
                .ToAsyncEnumerable()
                .ToArrayAsync(),
        };
        network.Links = network.Issues.SelectMany(x => x.Links).Distinct().ToList();

        var aggLinks = network.Links
            .Where(x => AggregationLinkTypes.Contains(x.Type))
            .GroupBy(x => x.Target)
            .Select(x => x.Key);
        var topLevelIssues = network.Issues
            .GroupBy(x => x.Id).Select(x => x.First())
            .Where(x => !aggLinks.Contains(x.Id));
        var issues = network.Issues.ToDictionary(x => x.Id, x => (data: x, check: false));
        foreach (var topLevelIssue in topLevelIssues)
            populateIssueLevel(issues, topLevelIssue, topId: topLevelIssue.Id);

        return network;
    }

    public void populateIssueLevel(
        Dictionary<string, (YTIssueModel data, bool check)> issues,
        YTIssueModel data,
        string topId,
        int level = 0)
    {
        if (!issues.TryGetValue(data.Id, out var issue) || issue.check) return;

        issue.data.TopId = topId;
        issue.data.Level = level;
        issue.check = true;

        foreach (var targetId in issue.data.Links
            .Where(x => AggregationLinkTypes.Contains(x.Type))
            .Where(x => x.Source == issue.data.Id)
            .Select(x => x.Target))
        {
            if (!issues.TryGetValue(targetId, out var target)) continue;

            populateIssueLevel(issues, target.data, topId, level + 1);
        }
    }
}
