using System;
using YouTrackSharp.Issues;

namespace NodeView.Domain;

public class YTIssueModel
{
    public string Id { get; set; }
    public string ProjectId { get; set; }
    public string Summary { get; set; }
    public IEnumerable<YTIssueLinkModel> Links { get; set; }
    public string TopId { get; set; }
    public int Level { get; set; }
}

public struct YTIssueLinkModel
{
    public string Type { get; set; }
    public string Source { get; set; }
    public string Target { get; set; }
}

public class YTIssueNetworkModel
{
    public SearchOptions Search { get; set; }
    public IEnumerable<YTIssueModel> Issues { get; set; }
    public IEnumerable<YTIssueLinkModel> Links { get; set; }
}
