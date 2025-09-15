using RabbitMQ.Client;

namespace Cyclone.Common.SimpleSoftDelete.Abstractions;

public interface IRabbitConnectionManager : IDisposable
{
    IConnection GetConnection(CancellationToken ct = default);
}
