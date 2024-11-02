using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

public class BaseDbEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Column("inserted_at", TypeName = "timestamp without time zone")]
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;

    [Column("last_modified", TypeName = "timestamp without time zone")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}