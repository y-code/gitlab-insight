using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using YouTrackInsight.Domain;
using YouTrackInsight.Entity;
using YouTrackInsight.Services;

namespace YouTrackInsight.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YouTrackController : ControllerBase
{
    private readonly YouTrackClientService _client;
    private readonly YTIssueImportService _issueImportService;

    public YouTrackController(YouTrackClientService client, YTIssueImportService issueImportService)
    {
        _client = client;
        _issueImportService = issueImportService;
    }

    [HttpGet("issue-network")]
    public async Task<YTIssueNetworkModel> GetIssueNetwork([FromQuery] SearchOptions options)
    {
        var issues = await _client.GetIssueNetwork(options);
        return issues;
    }

    [HttpGet("issue-import")]
    public IAsyncEnumerable<YTIssueImportTask> GetIssueImportTasks([FromQuery] IEnumerable<Guid> id)
        => _issueImportService.GetTasksInProgress();

    public class SubmitIssueImportRequest
    {
        public Guid Id { get; set; }
    }

    [HttpPut("issue-import")]
    public async Task<ActionResult> SubmitIssueImport([FromBody] SubmitIssueImportRequest request, CancellationToken ct)
    {
        try
        {
            await _issueImportService.SubmitTaskAsync(request.Id, ct);
        }
        catch (ArgumentException e)
        {
            return Problem(e.Message, statusCode: (int)HttpStatusCode.BadRequest);
        }
        catch (InvalidOperationException e)
        {
            return Problem(e.Message);
        }
        return Ok();
    }

    public class CancelIssueImportRequest
    {
        public Guid Id { get; set; }
    }

    [HttpDelete("issue-import")]
    public async Task<ActionResult> CancelIssueImport([FromBody] CancelIssueImportRequest request, CancellationToken ct)
    {
        try
        {
            await _issueImportService.CancelTaskAsync(request.Id, ct);
        }
        catch (ArgumentException e)
        {
            return Problem(e.Message, statusCode: (int)HttpStatusCode.BadRequest);
        }
        catch (InvalidOperationException e)
        {
            return Problem(e.Message);
        }
        return Ok();
    }
}

