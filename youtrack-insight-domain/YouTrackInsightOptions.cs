using System;

namespace YouTrackInsight.Domain;

public class YouTrackInsightOptions
{
    public static readonly string ConfigSectionName = "YouTrackInsight";

    public string? ApiEndpoint { get; set; }
    public string? ApiToken { get; set; }

    public YTIssueImportOptions IssueImport { get; set; } = new();
}

public class YTIssueImportOptions {
    public int MaxBacklogTasks { get; set; } = 3;
    public int MaxParallelTasks { get; set; } = 1;
}
