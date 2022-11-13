using System;
using Microsoft.AspNetCore.Mvc;
using YouTrackInsight.Domain;
using YouTrackInsight.Services;

namespace YouTrackInsight.Controllers;

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

    public class IssueImportOperationModel
    {
        public enum OperationType
        {
            Start,
            Cancel,
        }

        public Guid Id { get; set; }
        public OperationType Type { get; set; }
    }

    [HttpPost("issue-import")]
    public async Task ControlIssueImport([FromBody] IssueImportOperationModel operation)
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

