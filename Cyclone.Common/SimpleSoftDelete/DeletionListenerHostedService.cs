// DeletionListenerHostedService.cs

using Cyclone.Common.SimpleSoftDelete.Abstractions;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cyclone.Common.SimpleSoftDelete;

public sealed class DeletionListenerHostedService(
    ITopicEventReceiver receiver,
    IDeletionSubscriptionRegistry registry,
    IServiceProvider services,
    ILogger<DeletionListenerHostedService> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Starting deletion listener.");
        var all = registry.GetAll();

        foreach (var (topic, handlers) in all)
        {
            Task.Run(async () =>
            {
                var stream = await receiver.SubscribeAsync<DeletionEvent>(topic, stoppingToken);

                await foreach (var ev in stream.ReadEventsAsync().WithCancellation(stoppingToken))
                {
                    foreach (var h in handlers)
                    {
                        logger.LogDebug($"{handlers.Count} handler found for {topic}");

                        try
                        {
                            await h(ev, services, stoppingToken); 
                            logger.LogDebug($"{handlers.Count} handler processed for {topic}");
                        }
                        catch (Exception ex)
                        {
                            if (ex != null) logger.LogError(ex, ex.Message);
                        }
                    }
                }
            }, stoppingToken);
        }

        return Task.CompletedTask;
    }
}