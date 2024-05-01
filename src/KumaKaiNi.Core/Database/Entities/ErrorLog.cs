using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("error_logs")]
public class ErrorLog : BaseDbEntity
{
    [Column("timestamp", TypeName = "timestamp without time zone")]
    public DateTime Timestamp { get; set; }

    [Column("source")]
    public string? Source { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("stack_trace")]
    public string? StackTrace { get; set; }
    
    public ErrorLog() { }
        
    public ErrorLog(Exception ex)
    {
        Timestamp = DateTime.UtcNow;
        Source = ex.Source;
        Message = ex.Message;
        StackTrace = ex.StackTrace;
    }
}
