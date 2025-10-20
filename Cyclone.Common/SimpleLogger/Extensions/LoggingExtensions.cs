using Cyclone.Common.SimpleLogger.Configuration;
using Cyclone.Common.SimpleLogger.Middleware;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Cyclone.Common.SimpleLogger.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder UseSimpleLogging(
        this IHostBuilder builder,
        string serviceName,
        string connectionString,
        string logFilePath = "Logs/log-.txt")
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            var logger = LoggingConfiguration.CreateLogger(
                serviceName,
                connectionString,
                logFilePath);

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .WriteTo.Logger(logger);
        });
    }

    public static IApplicationBuilder UseSimpleRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }

    public static IRequestExecutorBuilder AddSimpleGraphQlLogging(
        this IRequestExecutorBuilder builder)
    {
        return builder.AddDiagnosticEventListener<GraphQlLoggingDiagnosticEventListener>();

    }
}