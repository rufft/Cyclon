using System.Linq.Expressions;
using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cyclone.Common.SimpleSoftDelete.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeletionPublisher(this IServiceCollection services)
    {
        services.AddScoped<IDeletionEventPublisher, HcDeletionEventPublisher>();
        return services;
    }
    
     /// <summary>
    /// Подписка на удаление родителя TParent. При событии вызывает стандартный SoftDeleteCascadeAsync
    /// по Id родителя в указанном DbContext.
    /// Имя подписки по умолчанию: "On{TParent}Delete".
    /// </summary>
    public static IServiceCollection AddSubscriptionForPolicy<TParent, TDbContext>(
        this IServiceCollection services,
        string? subscriptionName = null,
        string? deletedBy = null)
        where TDbContext : SimpleDbContext
        where TParent : BaseEntity
    {
        var name = subscriptionName ?? $"On{typeof(TParent).Name}Delete";

        return services.AddSubscription(name, async (ev, sp, ct) =>
        {
            // игнор своих же событий (по желанию)
            // var self = Assembly.GetEntryAssembly()!.GetName().Name!;
            // if (ev.OriginService == self) return;

            if (!string.Equals(ev.EntityType, typeof(TParent).Name, StringComparison.Ordinal))
                return;

            var factory = sp.GetRequiredService<IDbContextFactory<TDbContext>>();
            await using var db = await factory.CreateDbContextAsync(ct);

            // ВАЖНО: ниже вызов твоего стандартного метода каскада.
            // Предполагается, что он у тебя есть как extension на DbSet<TParent>.
            // Например: await db.Set<TParent>().SoftDeleteCascadeAsync(ev.EntityId, deletedBy, ct);
            // Я оставляю вызов в виде динамики, чтобы не ломать сборку, если сигнатура чуть отличается.
            var set = db.Set<TParent>();
            var method = set.GetType().GetMethods()
                .FirstOrDefault(m => m.Name == nameof(DbSetSoftDeleteExtensions.SoftDeleteCascadeAsync)
                                     && m.GetParameters().Length >= 2);
            if (method is null)
                throw new MissingMethodException($"Не найден {nameof(DbSetSoftDeleteExtensions.SoftDeleteCascadeAsync)} для TParent");

            var args = method.GetParameters().Length switch
            {
                2 => new object?[] { ev.EntityId, ct },
                3 => new object?[] { ev.EntityId, deletedBy, ct },
                _ => new object?[] { ev.EntityId, deletedBy, ct }
            };
            var task = (Task)method.Invoke(set, args)!;
            await task.ConfigureAwait(false);
        });
    }
    
    /// <summary>
    /// Подписка на удаление родителя с именем entityTypeName.
    /// При событии мягко помечает удалёнными детей TChild, у которых fkSelector == ev.EntityId.
    /// Имя подписки по умолчанию: "On{entityTypeName}Delete".
    /// </summary>
    public static IServiceCollection AddSubscriptionForForeignKey<TChild, TDbContext>(
        this IServiceCollection services,
        string entityTypeName,
        Expression<Func<TChild, Guid>> fkSelector,
        string? subscriptionName = null,
        string? deletedBy = null)
        where TChild : BaseEntity
        where TDbContext : SimpleDbContext
    {
        var name = subscriptionName ?? $"On{entityTypeName}Delete";

        return services.AddSubscription(name, async (ev, sp, ct) =>
        {
            if (!string.Equals(ev.EntityType, entityTypeName, StringComparison.Ordinal))
                return;

            var factory = sp.GetRequiredService<IDbContextFactory<TDbContext>>();
            await using var db = await factory.CreateDbContextAsync(ct);

            var set = db.Set<TChild>();
            
            var method = set.GetType().GetMethods()
                .FirstOrDefault(m => m.Name == nameof(DbSetSoftDeleteExtensions.SoftDeleteAsync)
                                     && m.GetParameters().Length >= 2);
            if (method is null)
                throw new MissingMethodException($"Не найден {nameof(DbSetSoftDeleteExtensions.SoftDeleteAsync)} для сущности");
        });
    }

    public static IServiceCollection AddSubscription(this IServiceCollection services,
        string subscriptionNameOrTopic,
        DeletionEventHandler handler)
    {
        services.TryAddSingleton<IDeletionSubscriptionRegistry, DeletionSubscriptionRegistry>();

        // Регистрируем «конфигуратор» как отдельный singleton, чтобы НЕ перерегистрировать сам реестр
        services.AddSingleton<IStartupFilter>(new SubscribeStartupFilter(subscriptionNameOrTopic, handler));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, DeletionListenerHostedService>());
        return services;
    }

    public static IServiceCollection AddEntityDeletionSubscription(this IServiceCollection services,
        string entityType,
        DeletionEventHandler handler)
        => services.AddSubscription(DeletionTopics.For(entityType), handler);
}
public sealed class SubscribeStartupFilter : IStartupFilter
{
    private readonly string _topic;
    private readonly DeletionEventHandler _handler;

    public SubscribeStartupFilter(string topic, DeletionEventHandler handler)
        => (_topic, _handler) = (topic, handler);

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => app =>
        {
            var reg = app.ApplicationServices.GetRequiredService<IDeletionSubscriptionRegistry>();
            reg.Subscribe(_topic, _handler);
            next(app);
        };
}