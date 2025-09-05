using Cyclone.Common.SimpleSoftDelete.Abstractions;
using HotChocolate.Subscriptions;

namespace Cyclone.Common.SimpleSoftDelete;

public sealed class HcDeletionEventPublisher(ITopicEventSender sender) : IDeletionEventPublisher
{
    public Task PublishAsync(DeletionEvent ev, CancellationToken ct = default) =>
        sender.SendAsync(DeletionTopics.For(ev.EntityType), ev, ct).AsTask();

    public Task PublishAsync<T>(Guid id, string originService,
        string? reason = null, bool cascade = true,
        string? correlationId = null, CancellationToken ct = default)
    {
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