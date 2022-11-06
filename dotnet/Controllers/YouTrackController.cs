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

}

