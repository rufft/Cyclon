using System.ComponentModel.DataAnnotations;
using Cyclone.Common.SimpleEntity;

namespace Cyclone.Common.SimpleLogger.Models;

public class LogEntry : BaseEntity
{
    [MaxLength(50)]
    public string Level { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public string? MessageTemplate { get; set; }
    
    public string? Exception { get; set; }
    
    public string? Properties { get; set; } // JSON
    
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    [MaxLength(200)]
    public string? RequestPath { get; set; }
    
    [MaxLength(50)]
    public string? OperationName { get; set; } // для GraphQL
    
    public int? ResponseTimeMs { get; set; }
    
    [MaxLength(50)]
    public string? SourceContext { get; set; }
    
    [MaxLength(100)]
    public string? MachineName { get; set; }
    
    public string? StackTrace { get; set; }
}