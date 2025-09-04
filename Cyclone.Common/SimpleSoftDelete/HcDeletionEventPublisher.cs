using Cyclone.Common.SimpleSoftDelete.Abstractions;
using HotChocolate.Subscriptions;

namespace Cyclone.Common.SimpleSoftDelete;

public class HcDeletionEventPublisher(ITopicEventSender sender) : IDeletionEventPublisher
{
    public Task PublishAsync(DeletionEvent ev, CancellationToken ct = default)
        => sender.SendAsync(DeletionTopics.For(ev.EntityType), ev, ct).AsTask();

    public Task PublishAsync<T>(Guid id, string originService,
        string? reason = null, bool cascade = true, string? correlationId = null,
        CancellationToken ct = default)
        => PublishAsync(new DeletionEvent(
            EntityType: typeof(T).Name,
            EntityId: id,
            OriginService: originService,
            Reason: reason,
            CorrelationId: correlationId ?? Guid.NewGuid().ToString("N"),
            OccurredAt: DateTime.Now,
            Cascade: cascade
        ), ct);
}