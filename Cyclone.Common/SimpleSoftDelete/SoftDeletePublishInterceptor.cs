using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cyclone.Common.SimpleSoftDelete;

public sealed class SoftDeletePublishInterceptor(IDeletionEventPublisher publisher,
    string originService) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var ctx = eventData.Context;
        if (ctx is null) return await base.SavingChangesAsync(eventData, result, ct);

        var toPublish = ctx.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Modified
                        && e.Properties.Any(p => p.Metadata.Name == "IsDeleted"
                                                 && p is { OriginalValue: false, CurrentValue: true }))
            .Select(e => new { e.Entity, Type = e.Entity.GetType(), Id = GetId(e) })
            .Where(x => x.Id != Guid.Empty)
            .ToList();

        var res = await base.SavingChangesAsync(eventData, result, ct);

        foreach (var x in toPublish)
        {
            await publisher.PublishAsync(
                new DeletionEvent(
                    EntityType: x.Type.Name,
                    EntityId: x.Id,
                    OriginService: originService,
                    Reason: "SoftDelete",
                    CorrelationId: Guid.NewGuid().ToString("N"),
                    OccurredAt: DateTime.Now,
                    Cascade: true),
                ct);
        }

        return res;

        static Guid GetId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry e)
            => e.Property("Id").CurrentValue is Guid g ? g : Guid.Empty;
    }
}
