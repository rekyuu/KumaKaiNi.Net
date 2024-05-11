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

    [Column("application_name")]
    public string ApplicationName { get; set; } = KumaConfig.ApplicationName;

    [Column("application_version")]
    public string ApplicationVersion { get; set; } = KumaConfig.ApplicationVersion;
    
    public ErrorLog() { }
        
    public ErrorLog(Exception ex)
    {
        Timestamp = DateTime.UtcNow;
        Source = ex.Source;
        Message = ex.Message;
        StackTrace = ex.StackTrace;
    }
}
