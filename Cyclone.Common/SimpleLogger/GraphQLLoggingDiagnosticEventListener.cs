using System.Collections.Concurrent;
using System.Diagnostics;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Cyclone.Common.SimpleLogger;

public class GraphQlLoggingDiagnosticEventListener(ILogger logger) : ExecutionDiagnosticEventListener
{
    private readonly ConcurrentDictionary<string, Stopwatch> _timers = new();

    public override void RequestError(RequestContext context, Exception exception)
    {
        logger.Error(exception, 
            "GraphQL request error for operation {OperationName}",
            context.Request.OperationName ?? "Unknown");
    }

    public override IDisposable ExecuteRequest(RequestContext context)
    {
        var operationName = context.Request.OperationName ?? "UnknownOperation";
        var sw = Stopwatch.StartNew();
        var requestId = context.ContextData.TryGetValue("RequestId", out var id) 
            ? id?.ToString() 
            : Guid.NewGuid().ToString();

        _timers[requestId!] = sw;

        return LogContext.Push(
            new PropertyEnricher("OperationName", operationName),
            new PropertyEnricher("CorrelationId", requestId!));
    }

    public override void StopProcessing(RequestContext? context)
    {
        
        if (context is null)
        {
            return;
        }

        var requestId = context.ContextData.TryGetValue("RequestId", out var id) 
            ? id?.ToString() 
            : null;

        if (requestId == null || !_timers.TryRemove(requestId, out var sw)) return;
        sw.Stop();
            
        var hasErrors = context.Result?.ContextData?.Count > 0;
        var level = hasErrors ? LogEventLevel.Warning : LogEventLevel.Information;

        using (LogContext.PushProperty("ResponseTimeMs", sw.ElapsedMilliseconds))
        {
            logger.Write(level,
                "GraphQL operation {OperationName} completed in {Elapsed:0.0000}ms with {ErrorCount} errors",
                context.Request.OperationName ?? "Unknown",
                sw.Elapsed.TotalMilliseconds,
                context.Result?.ContextData?.Count ?? 0);
        }
    }

    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        logger.Warning(
            "GraphQL resolver error in {FieldName}: {ErrorMessage}",
            context.Selection.Field.Name,
            error.Message);
    }
}

// Helper
internal class PropertyEnricher(string name, object value) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var property = propertyFactory.CreateProperty(name, value);
        logEvent.AddOrUpdateProperty(property);
    }
}