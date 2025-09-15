using System.Collections.ObjectModel;
using System.Reflection;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Cyclone.Common.SimpleSoftDelete.RabbitMQ;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cyclone.Common.SimpleSoftDelete.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqSoftDelete(
        this IServiceCollection services,
        Action<RabbitMqOptions>? configure = null)
    {
        if (configure != null) services.Configure(configure);
        else services.Configure<RabbitMqOptions>(_ => { });
        
        services.TryAddSingleton<ConnectionFactory>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            return new ConnectionFactory
            {
                HostName = opt.Host,
                Port = opt.Port,
                UserName = opt.User,
                Password = opt.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                TopologyRecoveryEnabled = true
            };
        });

        services.TryAddSingleton<IRabbitConnectionManager, RabbitConnectionManager>();

        // Паблишер (singleton)
        services.TryAddSingleton<IDeletionEventPublisher, RabbitMqDeletionEventPublisher>();

        // Регистрируем реестр подписок и hosted-listener
        services.TryAddSingleton<IDeletionSubscriptionRegistry, DeletionSubscriptionRegistry>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, RabbitMqDeletionListenerHostedService>());

        return services;
    }

    /// <summary>
    /// Упрощённая регистрация подписки (аналог вашей AddSubscription, но без HotChocolate).
    /// </summary>
    public static IServiceCollection AddDeletionSubscription(
        this IServiceCollection services,
        string subscriptionNameOrTopic,
        DeletionEventHandler handler)
    {
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