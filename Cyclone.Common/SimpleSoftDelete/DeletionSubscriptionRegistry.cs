using System.Collections.Concurrent;
using Cyclone.Common.SimpleSoftDelete.Abstractions;

namespace Cyclone.Common.SimpleSoftDelete;

public delegate Task DeletionEventHandler(DeletionEvent ev, IServiceProvider services, CancellationToken ct);

internal sealed class DeletionSubscriptionRegistry : IDeletionSubscriptionRegistry
{
    private readonly ConcurrentDictionary<string, List<DeletionEventHandler>> _map = new(StringComparer.Ordinal);

    public void SubscribeTopic(string topic, DeletionEventHandler handler)
    {
        var list = _map.GetOrAdd(topic, _ => []);
        lock (list) list.Add(handler);
    }

    public void Subscribe(string subscriptionName, DeletionEventHandler handler)
    {
        var topic = subscriptionName switch
        {
            "OnDisplayDelete"     => DeletionTopics.For("Display"),
            "OnDisplayTypeDelete" => DeletionTopics.For("DisplayType"),
            _ => subscriptionName
        };
        SubscribeTopic(topic, handler);
    }

    public IReadOnlyDictionary<string, List<DeletionEventHandler>> GetAll() => _map!;
}
