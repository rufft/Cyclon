using System.Collections.ObjectModel;
using System.Reflection;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cyclone.Common.SimpleSoftDelete.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeletionPublisher(this IServiceCollection services)
    {
        services.TryAddScoped<IDeletionEventPublisher, HcDeletionEventPublisher>();
        return services;
    }

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

    public static IRequestExecutorBuilder AddDeletionSubscriptions(this IRequestExecutorBuilder builder)
        => builder.AddInMemorySubscriptions();

    public static IServiceCollection AddSubscription(
        this IServiceCollection services,
        string subscriptionNameOrTopic,
        DeletionEventHandler handler)
    {
        services.TryAddSingleton<IDeletionSubscriptionRegistry, DeletionSubscriptionRegistry>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, DeletionListenerHostedService>());

        services.AddOptions<DeletionSubscriptionOptions>()
            .PostConfigure(o => o.Handlers.Add((subscriptionNameOrTopic, handler)));

        return services;
    }
}

public sealed class DeletionSubscriptionOptions
{
    internal readonly List<(string nameOrTopic, DeletionEventHandler handler)> Handlers = new();

    public IReadOnlyCollection<(string, DeletionEventHandler)> Items =>
        new ReadOnlyCollection<(string, DeletionEventHandler)>(Handlers);
}