namespace Cyclone.Common.SimpleSoftDelete.Abstractions;

public interface IDeletionEventPublisher
{
    Task PublishAsync(DeletionEvent ev, CancellationToken ct = default);
    Task PublishAsync<T>(Guid id, string originService,
        string? reason = null, bool cascade = true, string? correlationId = null,
        CancellationToken ct = default);
}