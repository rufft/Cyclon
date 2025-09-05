using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cyclone.Common.SimpleSoftDelete;

public static class SoftDeleteSubscriptionExtensions
{
    public static IServiceCollection AddSoftDeleteEventSystem(
        this IServiceCollection services, 
        string originServiceName,
        Action? policies = null)
    {
        services.AddScoped<IDeletionEventPublisher, HcDeletionEventPublisher>();
        
        services.AddScoped<SoftDeletePublishInterceptor>(sp => 
            new SoftDeletePublishInterceptor(
                sp.GetRequiredService<IDeletionEventPublisher>(), 
                originService: originServiceName));
        
        policies?.Invoke();
        
        return services;
    }
    public static IServiceCollection AddSoftDeleteEventSystem(
        this IServiceCollection services,
        Action? policies = null)
    {
        services.AddScoped<IDeletionEventPublisher, HcDeletionEventPublisher>();

        services.AddScoped<SoftDeletePublishInterceptor>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var originName = env.ApplicationName;

            return new SoftDeletePublishInterceptor(
                sp.GetRequiredService<IDeletionEventPublisher>(),
                originService: originName);
        });

        policies?.Invoke();
        return services;
    }
}
