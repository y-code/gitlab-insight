using System;

namespace GitLabInsight.Domain;

public class GitLabInsightOptions
{
    public static readonly string ConfigSectionName = "GitLabInsight";

    public string ApiEndpoint { get; set; }
    public string ApiToken { get; set; }
}
