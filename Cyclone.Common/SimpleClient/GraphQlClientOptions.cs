namespace Cyclone.Common.SimpleClient;

public sealed class GraphQlClientOptions
{
    public required string Endpoint { get; init; } // например: https://svc1.internal/graphql
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
}