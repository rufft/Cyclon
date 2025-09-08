using System.Collections.Concurrent;
using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cyclone.Common.SimpleSoftDelete;

public sealed class SoftDeletePublishInterceptor(
    IDeletionEventPublisher publisher,
    string originService
) : SaveChangesInterceptor
{
    private const string SoftDeletedKey = "__SoftDeletedEntities";
    
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var softDeleted = ctx.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Modified
                        && HasBool(e, "IsDeleted")
                        && !GetOriginalBool(e, "IsDeleted")
                        && GetCurrentBool(e, "IsDeleted"))
            .Select(e => new EntityInfo(e.Entity.GetType() ,GetGuid(e, "Id")))
            .Where(t => t.EntityId != Guid.Empty)
            .ToList();

        TempStore.Set(ctx, SoftDeletedKey, softDeleted);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is null) return await base.SavedChangesAsync(eventData, result, cancellationToken);
        try
        {
            if (result > 0 &&
                TempStore.TryGet<List<EntityInfo>>(ctx,nameof(SoftDeletePublishInterceptor), out var listObj) == true &&
                listObj is { Count: > 0 })
            {
                foreach (var entityInfo in listObj)
                {
                    var method = typeof(IDeletionEventPublisher).GetMethod(nameof(IDeletionEventPublisher.PublishAsync), 4, [])!;
                    var gen = typeof(IDeletionEventPublisher).GetMethods()
                        .First(m => m is { IsGenericMethod: true, Name: nameof(IDeletionEventPublisher.PublishAsync) } &&
                                    m.GetGenericArguments().Length == 1 &&
                                    m.GetParameters().Length >= 2)
                        .MakeGenericMethod(entityInfo.EntityType);
                    var task = (Task)gen.Invoke(publisher, [entityInfo.EntityId, originService, "SoftDelete", true, null, cancellationToken
                    ])!;
                    await task.ConfigureAwait(false);
                }
            }
        }
        finally
        {
            TempStore.Remove(ctx, SoftDeletedKey);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static bool HasBool(EntityEntry e, string name) =>
        e.Metadata.FindProperty(name)?.ClrType == typeof(bool);
    private static bool GetOriginalBool(EntityEntry e, string name) =>
        (bool)(e.Property(name).OriginalValue ?? false);
    private static bool GetCurrentBool(EntityEntry e, string name) =>
        (bool)(e.Property(name).CurrentValue ?? false);

    private static Guid GetGuid(EntityEntry e, string name)
    {
        var p = e.Metadata.FindProperty(name);
        if (p?.ClrType != typeof(Guid)) return Guid.Empty;
        var v = e.Property(name).CurrentValue;
        return v is Guid g ? g : Guid.Empty;
    }
}

public record EntityInfo(Type EntityType, Guid EntityId);