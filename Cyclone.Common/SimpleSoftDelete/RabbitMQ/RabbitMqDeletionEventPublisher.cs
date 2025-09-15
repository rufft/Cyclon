using System.Text;
using System.Text.Json;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cyclone.Common.SimpleSoftDelete.RabbitMQ;

internal sealed class RabbitMqDeletionEventPublisher(
    IRabbitConnectionManager connMgr,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqDeletionEventPublisher> logger) : IDeletionEventPublisher
{
    private readonly RabbitMqOptions _opt = options.Value;

    public Task PublishAsync(DeletionEvent ev, CancellationToken ct = default)
        => PublishInternalAsync(ev, ct);

    public Task PublishAsync<T>(Guid id, string originService, string? reason = null,
        bool cascade = true, string? correlationId = null, CancellationToken ct = default)
    {
        var ev = new DeletionEvent(
            typeof(T).Name, id, originService, reason,
            correlationId ?? Guid.NewGuid().ToString("N"),
            DateTime.Now, cascade);
        return PublishInternalAsync(ev, ct);
    }

    private async Task PublishInternalAsync(DeletionEvent ev, CancellationToken ct)
    {
        var routingKey = $"{ev.EntityType}.Deleted";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ev));

        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var connection = connMgr.GetConnection(ct);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
                await channel.ExchangeDeclareAsync(_opt.Exchange, type: "topic", durable: true, autoDelete: false, cancellationToken: ct);

                await channel.BasicPublishAsync(_opt.Exchange, routingKey, body, cancellationToken: ct);

                logger.LogDebug("Published deletion event {RoutingKey} {EntityId}", routingKey, ev.EntityId);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Publish attempt {Attempt}/{Max} failed for {RoutingKey}", attempt, maxAttempts, routingKey);
                if (attempt == maxAttempts) throw;
                await Task.Delay(TimeSpan.FromSeconds(1 * attempt), ct);
            }
        }
    }
}