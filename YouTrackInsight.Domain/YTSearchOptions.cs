﻿using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using YouTrackInsight.Entity;

namespace YouTrackInsight.Domain;

public class YTSearchOptions
{
    [BindProperty(Name = "project")]
    [JsonPropertyName("project")]
    public string[] Projects { get; set; } = Array.Empty<string>();
}

public class YTIssueNetworkModel
{
    public YTSearchOptions Options { get; set; } = new();
    public IEnumerable<YTIssueModel> Issues { get; set; } = Enumerable.Empty<YTIssueModel>();
    public IEnumerable<YTIssueLinkModel> Links { get; set; } = Enumerable.Empty<YTIssueLinkModel>();
}