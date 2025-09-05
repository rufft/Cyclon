namespace Cyclone.Common.SimpleSoftDelete.Abstractions;

public interface IDeletionEventPublisher
{
    ValueTask PublishAsync(DeletionEvent ev, CancellationToken ct = default);
    ValueTask PublishAsync<T>(Guid id, string originService,
        string? reason = null, bool cascade = true,
        string? correlationId = null, CancellationToken ct = default);
}
