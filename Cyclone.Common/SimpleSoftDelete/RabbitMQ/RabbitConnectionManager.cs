using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Cyclone.Common.SimpleSoftDelete.RabbitMQ;

public sealed class RabbitConnectionManager(
    ConnectionFactory factory,
    ILogger<RabbitConnectionManager> logger) : IRabbitConnectionManager
{
    private readonly object _lock = new();
    private IConnection? _conn;

    public IConnection GetConnection(CancellationToken ct = default)
    {
        if (_conn is { IsOpen: true }) return _conn;

        lock (_lock)
        {
            if (_conn is { IsOpen: true }) return _conn;

            const int maxAttempts = 30; // ~30 * 1s = до 30 сек ожидания
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    _conn = factory.CreateConnectionAsync($"{AppDomain.CurrentDomain.FriendlyName}-conn", ct).GetAwaiter().GetResult();
                    return _conn;
                }
                catch (BrokerUnreachableException ex)
                {
                    logger.LogWarning(ex, "RabbitMQ not reachable, attempt {Attempt}/{Max}. Waiting…", attempt, maxAttempts);
                    if (attempt == maxAttempts) throw;
                    Task.Delay(TimeSpan.FromSeconds(1), ct).Wait(ct);
                }
            }

            throw new InvalidOperationException("Failed to connect to RabbitMQ.");
        }
    }

    public void Dispose()
    {
        try { _conn?.Dispose(); } catch { /* ignore */ }
    }
}