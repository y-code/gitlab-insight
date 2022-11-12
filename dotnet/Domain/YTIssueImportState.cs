using System;

namespace GitLabInsight.Domain;

public class YTIssueImportState
{
    public Guid Id { get; set; }
    public bool IsInProgress { get; set; }
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
}
