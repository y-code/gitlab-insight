using System;

namespace YouTrackInsight.Domain;

public class YouTrackInsightOptions
{
    public static readonly string ConfigSectionName = "YouTrackInsight";

    public string ApiEndpoint { get; set; }
    public string ApiToken { get; set; }
}
