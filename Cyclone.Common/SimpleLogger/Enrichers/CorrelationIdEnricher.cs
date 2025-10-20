// Cyclone.Common.Logging/Enrichers/CorrelationIdEnricher.cs

using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Cyclone.Common.SimpleLogger.Enrichers;

public class CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var correlationId = httpContext.TraceIdentifier;
        
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
        {
            correlationId = headerValue.ToString();
        }

        var property = propertyFactory.CreateProperty(CorrelationIdPropertyName, correlationId);
        logEvent.AddOrUpdateProperty(property);
    }
}