using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.PostgreSQL;

namespace Cyclone.Common.SimpleLogger.Configuration;

public static class LoggingConfiguration
{
    public static ILogger CreateLogger(
        string serviceName,
        string connectionString,
        string logFilePath = "Logs/log-.txt",
        LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        var columnOptions = new Dictionary<string, ColumnWriterBase>
        {
            { "message", new RenderedMessageColumnWriter() },
            { "message_template", new MessageTemplateColumnWriter() },
            { "level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
            { "timestamp", new TimestampColumnWriter() },
            { "exception", new ExceptionColumnWriter() },
            { "log_event", new LogEventSerializedColumnWriter() },
            { "service_name", new SinglePropertyColumnWriter("ServiceName", PropertyWriteMethod.Raw) },
            { "correlation_id", new SinglePropertyColumnWriter("CorrelationId", PropertyWriteMethod.Raw) },
            { "request_path", new SinglePropertyColumnWriter("RequestPath", PropertyWriteMethod.Raw) },
            { "operation_name", new SinglePropertyColumnWriter("OperationName", PropertyWriteMethod.Raw) },
            { "response_time_ms", new SinglePropertyColumnWriter("ResponseTimeMs", PropertyWriteMethod.Raw) },
            { "source_context", new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.Raw) },
            { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.Raw) }
        };

        return new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.FromLogContext()
            
            // Файловый лог (структурированный JSON)
            .WriteTo.Async(a => a.File(
                path: logFilePath,
                formatter: new JsonFormatter(),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100_000_000, // 100MB
                rollOnFileSizeLimit: true))
            
            // Файловый лог (текстовый для быстрого просмотра)
            .WriteTo.Async(a => a.File(
                path: "logs/log-readable-.txt",
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7))
            
            // PostgreSQL
            .WriteTo.Async(a => a.PostgreSQL(
                connectionString: connectionString,
                tableName: "logs",
                columnOptions: columnOptions,
                needAutoCreateTable: true,
                respectCase: true))
            
            // Console (для разработки)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            
            .CreateLogger();
    }
}