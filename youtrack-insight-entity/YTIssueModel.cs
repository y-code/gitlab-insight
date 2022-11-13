using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YouTrackInsight.Entity;

[Table("yt_issue")]
[PrimaryKey(nameof(Id), nameof(ProjectId))]
public class YTIssueModel
{
    [Column("id")]
    public string? Id { get; set; }
    [Column("project_id")]
    public string? ProjectId { get; set; }
    [Column("summary")]
    public string? Summary { get; set; }
    public ICollection<YTIssueLinkModel> Links { get; set; }
        = new Collection<YTIssueLinkModel>();
    [NotMapped]
    public string? TopId { get; set; }
    [NotMapped]
    public int Level { get; set; }
}

[Table("yt_issue_link")]
[PrimaryKey(nameof(Id))]
public class YTIssueLinkModel
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Column("type")]
    public string? Type { get; set; }
    [Column("source")]
    [ForeignKey(nameof(YTIssueLinkModel))]
    public string? Source { get; set; }
    [Column("target")]
    [ForeignKey(nameof(YTIssueLinkModel))]
    public string? Target { get; set; }
}
