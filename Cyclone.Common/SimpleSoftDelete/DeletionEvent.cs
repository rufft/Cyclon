namespace Cyclone.Common.SimpleSoftDelete;

public sealed record DeletionEvent(
    string EntityType,
    Guid EntityId,
    string OriginService,
    string? Reason,
    string CorrelationId,
    DateTime OccurredAt,
    bool Cascade  
);