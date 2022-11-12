using System;
using Microsoft.AspNetCore.Mvc;
using GitLabInsight.Domain;
using GitLabInsight.Services;

namespace GitLabInsight.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YouTrackController : ControllerBase
{
    private readonly YouTrackClientService _client;

    public YouTrackController(YouTrackClientService client)
    {
        this._client = client;
    }

    [HttpGet("issue-network")]
    public async Task<YTIssueNetworkModel> GetIssueNetwork([FromQuery] SearchOptions options)
    {
        var issues = await _client.GetIssueNetwork(options);
        return issues;
    }

    public enum IssueImportOperation
    {
        Start,
        Cancel,
    }

    [HttpPost("issue-import")]
    public async Task ControlIssueImport([FromBody] IssueImportOperation operation)
    {
    }

    [HttpGet("issue-import")]
    public async Task<IEnumerable<YTIssueImportState>> GetIssueImportStates([FromQuery] IEnumerable<Guid> id)
    {
        return id.Select(x => new YTIssueImportState
        {
            Id = x,
            Start = DateTimeOffset.UtcNow,
            End = null,
            IsInProgress = true,
            HasError = false,
            ErrorMessage = null,
        });
    }
}

