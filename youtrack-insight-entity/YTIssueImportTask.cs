using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace YouTrackInsight.Entity;

[Table("issue_import_task")]
public class YTIssueImportTask
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("start")]
    public DateTimeOffset? Start { get; set; }
    [Column("end")]
    public DateTimeOffset? End { get; set; }
    [Column("is_cancelled")]
    public bool IsCancelled { get; set; }
    [Column("has_error")]
    public bool HasError { get; set; }
    [Column("message")]
    public string? Message { get; set; }
}
