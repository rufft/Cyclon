using Cyclone.Common.SimpleSoftDelete.Abstractions;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Logging;

namespace Cyclone.Common.SimpleSoftDelete;

public sealed class HcDeletionEventPublisher(ITopicEventSender sender, ILogger<HcDeletionEventPublisher> logger) : IDeletionEventPublisher
{
    public ValueTask PublishAsync(DeletionEvent ev, CancellationToken ct = default)
    {
        logger.LogCritical("Publishing deletion event {Service} for {Type}:{Id}",
            ev.OriginService, ev.EntityType, ev.EntityId);        
        logger.LogCritical("{arg}", DeletionTopics.For(ev.EntityType));
        return sender.SendAsync(DeletionTopics.For(ev.EntityType), ev, ct);
    }

    public ValueTask PublishAsync<T>(Guid id, string originService,
        string? reason = null, bool cascade = true,
        string? correlationId = null, CancellationToken ct = default)
    {
        logger.LogCritical("Publishing deletion event {Service} for {Type}:{Id}",
            originService, typeof(T).ToString(), id);
        var ev = new DeletionEvent(
            typeof(T).Name,
            id,
            originService,
            reason,
            correlationId ?? Guid.NewGuid().ToString("N"),
            DateTime.Now,
            cascade);
        return PublishAsync(ev, ct);
    }
}