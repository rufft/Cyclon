using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Cyclone.Common.SimpleLogger.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
            {
                correlationId = headerValue.ToString();
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
                context.Response.Headers["X-Correlation-ID"] = correlationId;
            }

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("UserId", context.User?.Identity?.Name ?? "Anonymous"))
            {
                var sw = Stopwatch.StartNew();

                try
                {
                    await next(context);  // здесь используется Microsoft.AspNetCore.Http.RequestDelegate
                    sw.Stop();

                    var level = context.Response.StatusCode >= 500
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;

                    logger.Write(level,
                        "HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0000}ms",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        sw.Elapsed.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    logger.Error(ex,
                        "HTTP {Method} {Path} failed after {Elapsed:0.0000}ms",
                        context.Request.Method,
                        context.Request.Path,
                        sw.Elapsed.TotalMilliseconds);
                    throw;
                }
            }
        }
    }
}
