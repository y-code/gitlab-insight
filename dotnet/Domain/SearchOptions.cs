using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace GitLabInsight.Domain;

public class SearchOptions
{
    [BindProperty(Name = "project")]
    [JsonPropertyName("project")]
    public string[] Projects { get; set; } = Array.Empty<string>();
}
