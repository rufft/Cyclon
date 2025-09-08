using System.Collections.Concurrent;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Hosting;

namespace Cyclone.Common.SimpleSoftDelete
{
    public delegate Task DeletionEventHandler(DeletionEvent ev, IServiceProvider services, CancellationToken ct);

public interface IDeletionSubscriptionRegistry
{
    void SubscribeTopic(string topic, DeletionEventHandler handler);
    void Subscribe(string subscriptionName, DeletionEventHandler handler);
    IReadOnlyDictionary<string, List<DeletionEventHandler>> GetAll();
}

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
            _ => subscriptionName // если уже полный топик
        };
        SubscribeTopic(topic, handler);
    }

    public IReadOnlyDictionary<string, List<DeletionEventHandler>> GetAll() => _map!;
}

public sealed class DeletionListenerHostedService(
    ITopicEventReceiver receiver,
    IDeletionSubscriptionRegistry registry,
    IServiceProvider services) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var all = registry.GetAll();

        foreach (var (topic, handlers) in all)
        {
            _ = Task.Run(async () =>
            {
                var stream = await receiver.SubscribeAsync<DeletionEvent>(topic, stoppingToken);
                await foreach (var ev in stream.ReadEventsAsync().WithCancellation(stoppingToken))
                {
                    foreach (var h in handlers)
                    {
                        try { await h(ev, services, stoppingToken); }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[DeletionListener] error on {topic}: {ex}");
                        }
                    }
                }
            }, stoppingToken);
        }

        return Task.CompletedTask;
    }
}
}

