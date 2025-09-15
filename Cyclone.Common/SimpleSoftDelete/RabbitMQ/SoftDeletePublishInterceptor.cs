using System.Collections.Concurrent;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cyclone.Common.SimpleSoftDelete.RabbitMQ;

public sealed class SoftDeletePublishInterceptor(string originService) : SaveChangesInterceptor
{
    private const string Key = "__SoftDeletedEntities";
    private static readonly ConcurrentDictionary<Guid, List<(Type, Guid)>> Temp = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var ctx = eventData.Context;
        if (ctx == null) return base.SavingChanges(eventData, result);
        var batch = ctx.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified
                        && e.Metadata.FindProperty("IsDeleted")?.ClrType == typeof(bool)
                        && !e.OriginalValues.GetValue<bool>("IsDeleted")
                        && e.CurrentValues.GetValue<bool>("IsDeleted"))
            .Select(e => (e.Entity.GetType(), Id: e.CurrentValues.GetValue<Guid>("Id")))
            .Where(t => t.Id != Guid.Empty)
            .ToList();

        if (batch.Count > 0)
            Temp.AddOrUpdate(ctx.ContextId.InstanceId, batch, (_, list) => { list.AddRange(batch); return list; });
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        var ctx = eventData.Context;
        if (result <= 0 || ctx == null || !Temp.TryRemove(ctx.ContextId.InstanceId, out var list))
            return await base.SavedChangesAsync(eventData, result, ct);
        var publisher = ctx.GetService<IDeletionEventPublisher>(); // резолвим из scope контекста
        foreach (var (type, id) in list)
        {
            await publisher.PublishAsync(new DeletionEvent(
                type.Name, id, originService,
                "SoftDelete", Guid.NewGuid().ToString("N"),
                DateTime.Now, true), ct);
        }
        return await base.SavedChangesAsync(eventData, result, ct);
    }
}