using System;
using System.Linq;
using Microsoft.Extensions.Options;
using NodeView.Domain;
using YouTrackSharp;
using YouTrackSharp.Issues;
using YouTrackSharp.Projects;

namespace NodeView.Services;

public class YouTrackClientService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<NodeViewOptions> _options;

    private Connection? _connection;
    private IProjectsService _projectsService;
    private IIssuesService _issuesService;

    public YouTrackClientService(IOptionsMonitor<NodeViewOptions> options, ILogger<YouTrackClientService> logger)
    {
        _options = options;
        _logger = logger;

        options.OnChange(o =>
        {
            ResetConnection();
        });
        ResetConnection();
    }

    private void ResetConnection()
    {
        _connection = new BearerTokenConnection(
            this._options.CurrentValue.ApiEndpoint,
            this._options.CurrentValue.ApiToken);
        _projectsService = _connection.CreateProjectsService();
        _issuesService = _connection.CreateIssuesService();
    }

    private async Task<ICollection<Project>> GetAccessibleProjects()
    {
        try
        {
            return await _projectsService.GetAccessibleProjects();
        }
        catch (UnauthorizedConnectionException e)
        {
            _logger.LogError(e, "YouTrack responded an error: {Response}", e.Response);
            throw;
        }
    }

    private async Task<ICollection<Issue>> GetIssuesInProject(string projectId)
    {
        try
        {
            return await _issuesService.GetIssuesInProject(projectId);
        }
        catch (UnauthorizedConnectionException e)
        {
            _logger.LogError(e, "YouTrack responded an error: {Response}", e.Response);
            throw;
        }
    }

    private async Task<IEnumerable<Link>> GetLinksForIssue(string issueId)
    {
        try
        {
            return await _issuesService.GetLinksForIssue(issueId);
        }
        catch (UnauthorizedConnectionException e)
        {
            _logger.LogError(e, "YouTrack responded an error: {Response}", e.Response);
            throw;
        }
    }

    public async IAsyncEnumerable<YTIssueModel> GetIssuesAsync()
    {
        var projects = await GetAccessibleProjects();
        foreach (var project in projects)
        {
            _logger.LogDebug("Project: {Project}", project.Name);
            var issues = await GetIssuesInProject(project.ShortName);
            foreach (var issue in issues)
            {
                _logger.LogDebug("Issue: {Issue}", issue.Summary);
                var links = await GetLinksForIssue(issue.Id);
                yield return new YTIssueModel
                {
                    Id = issue.Id,
                    ProjectId = project.ShortName,
                    Summary = issue.Summary,
                    Links = links.Select(x => new YTIssueLinkModel
                    {
                        Type = x.TypeName,
                        Source = x.Source,
                        Target = x.Target,
                    }),
                };
            }
        }
    }

    private readonly string[] AggregationLinkTypes = new[]
    {
        "Subtask",
        "Depend",
    };

    public async Task<YTIssueNetworkModel> GetIssueNetwork()
    {
        var network = new YTIssueNetworkModel
        {
            Issues = await GetIssuesAsync().ToListAsync(),
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
