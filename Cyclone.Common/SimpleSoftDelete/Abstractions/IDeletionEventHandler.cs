namespace Cyclone.Common.SimpleSoftDelete.Abstractions;

public interface IDeletionEventHandler
{
    Task HandleAsync(DeletionEvent ev, CancellationToken ct);
}