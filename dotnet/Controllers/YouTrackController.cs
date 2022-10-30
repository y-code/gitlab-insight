using System;
using Microsoft.AspNetCore.Mvc;
using NodeView.Domain;
using NodeView.Services;

namespace NodeView.Controllers;

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
    public async Task<YTIssueNetworkModel> GetIssueNetwork()
    {
        var issues = await _client.GetIssueNetwork();
        return issues;
    }

}

