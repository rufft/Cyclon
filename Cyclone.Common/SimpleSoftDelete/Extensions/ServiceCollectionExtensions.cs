using System.Reflection;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cyclone.Common.SimpleSoftDelete.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Регистрирует паблишер (для источника событий).</summary>
    public static IServiceCollection AddDeletionPublisher(this IServiceCollection services)
    {
        services.AddScoped<IDeletionEventPublisher, HcDeletionEventPublisher>();
        return services;
    }

    /// <summary>Добавляет SoftDeleteEventSystem: применяет политики и перехватчик.</summary>
    public static IServiceCollection AddSoftDeleteEventSystem(
        this IServiceCollection services,
        Action configurePolicies,
        string? originServiceName = null)
    {
        configurePolicies();
        var origin = originServiceName ?? Assembly.GetEntryAssembly()!.GetName().Name!;
        services.TryAddScoped(sp => new SoftDeletePublishInterceptor(
            sp.GetRequiredService<IDeletionEventPublisher>(), origin));
        return services;
    }

    /// <summary>Упрощённый способ добавить GraphQL‑подписки.</summary>
    public static IRequestExecutorBuilder AddDeletionSubscriptions(this IRequestExecutorBuilder builder)
        => builder.AddInMemorySubscriptions();

    /// <summary>Регистрирует подписку и обработчик одной строкой.</summary>
    public static IServiceCollection AddSubscription(
        this IServiceCollection services,
        string subscriptionNameOrTopic,
        DeletionEventHandler handler)
    {
        services.TryAddSingleton<IDeletionSubscriptionRegistry>(sp =>
        {
            if (sp.GetService<IDeletionSubscriptionRegistry>() is DeletionSubscriptionRegistry reg)
            {
                reg.Subscribe(subscriptionNameOrTopic, handler);
                return reg;
            }
            var newReg = new DeletionSubscriptionRegistry();
            newReg.Subscribe(subscriptionNameOrTopic, handler);
            return newReg;
        });

        services.AddHostedService<DeletionListenerHostedService>();
        return services;
    }
}