using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
    private readonly YouTrackInsightHubClients _hubClients;

    public YouTrackController(
        YouTrackClientService client,
        YTIssueImportService issueImportService,
        YouTrackInsightHubClients hubClients)
    {
        _client = client;
        _issueImportService = issueImportService;
        _hubClients = hubClients;
    }

    [HttpGet("issue-network")]
    public async Task<YTIssueNetworkModel> GetIssueNetwork([FromQuery] SearchOptions options)
    {
        var issues = await _client.GetIssueNetwork(options);
        return issues;
    }

    [HttpGet("issue-import")]
    public IAsyncEnumerable<YTIssueImportTask> GetIssueImportTasks([FromQuery] IEnumerable<Guid> id)
        => _issueImportService.GetTasksAsync();

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

        await _hubClients.NotifyIssueImportTaskUpdatedAsync(request.Id, ct);

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

        await _hubClients.NotifyIssueImportTaskUpdatedAsync(request.Id, ct);

        return Ok();
    }
}

