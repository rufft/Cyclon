using System.Text;
using System.Text.Json;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Cyclone.Common.SimpleSoftDelete.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cyclone.Common.SimpleSoftDelete.RabbitMQ;

public sealed class RabbitMqDeletionListenerHostedService(
    IRabbitConnectionManager connMgr,
    IDeletionSubscriptionRegistry registry,
    IOptions<DeletionSubscriptionOptions> opts,
    IOptions<RabbitMqOptions> rmqOpts,
    IServiceProvider services,
    ILogger<RabbitMqDeletionListenerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Сначала прогоним все зарегистрированные "читаемые" подписки в реестр (name -> topic)
        foreach (var (nameOrTopic, handler) in opts.Value.Items)
        {
            registry.Subscribe(nameOrTopic, handler);
        }

        var all = registry.GetAll(); // topic -> handlers
        if (all.Count == 0)
        {
            logger.LogInformation("No deletion subscriptions registered.");
            return;
        }

        var connection = connMgr.GetConnection(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var opt = rmqOpts.Value;
        var exchange = opt.Exchange;
        await channel.ExchangeDeclareAsync(exchange, "topic", durable: true, autoDelete: false, cancellationToken: stoppingToken);

        var queueName = opt.QueueName ?? AppDomain.CurrentDomain.FriendlyName;
        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        // Биндим все топики этого сервиса
        foreach (var topic in all.Keys)
        {
            await channel.QueueBindAsync(queue: queueName, exchange: exchange, routingKey: topic, cancellationToken: stoppingToken);
            logger.LogInformation("Queue {Queue} bound to {Exchange} with key {Key}", queueName, exchange, topic);
        }

        await channel.BasicQosAsync(0, 1, false, stoppingToken); // fair dispatch

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var routingKey = ea.RoutingKey; // например "Display.Deleted"
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var ev = JsonSerializer.Deserialize<DeletionEvent>(json);

                if (ev is null)
                {
                    logger.LogWarning("Skip message with null body. RK={RoutingKey}", routingKey);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                if (!registry.GetAll().TryGetValue(routingKey, out var handlers) || handlers.Count == 0)
                {
                    // нет подписчиков — просто ack чтобы не копилось
                    logger.LogDebug("No handlers for {RoutingKey}", routingKey);
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                foreach (var h in handlers)
                {
                    try { await h(ev, services, stoppingToken); }
                    catch (Exception hex)
                    {
                        // По-умолчанию не реqueue, чтобы не зациклить. Логируем.
                        logger.LogError(hex, "Handler error for {RoutingKey}", routingKey);
                    }
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Consumer error");
                // Можно Nack с requeue=false, чтобы не крутить «ядовитые» сообщения
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }
}