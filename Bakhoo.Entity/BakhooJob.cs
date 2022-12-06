using System.ComponentModel.DataAnnotations.Schema;

namespace Bakhoo.Entity;

[Table("job", Schema = "bakhoo")]
public class BakhooJob
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("submitted")]
    public DateTimeOffset? Submitted { get; set; }
    [Column("start")]
    public DateTimeOffset? Start { get; set; }
    [Column("end")]
    public DateTimeOffset? End { get; set; }
    [Column("is_cancelling")]
    public bool IsCancelling { get; set; }
    [Column("cancel_requested")]
    public DateTimeOffset? CancelRequested { get; set; }
    [Column("is_cancelled")]
    public bool IsCancelled { get; set; }
    [Column("has_error")]
    public bool HasError { get; set; }
    [Column("message")]
    public string? Message { get; set; }
    [Column("type")]
    public string? Type { get; set; }
    [Column("data", TypeName = "jsonb")]
    public string? Data { get; set; }
}
