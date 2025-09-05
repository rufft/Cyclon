// DeletionListenerHostedService.cs

using Cyclone.Common.SimpleSoftDelete.Abstractions;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Hosting;

namespace Cyclone.Common.SimpleSoftDelete;

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
                            // TODO Logger
                        }
                    }
                }
            }, stoppingToken);
        }

        return Task.CompletedTask;
    }
}