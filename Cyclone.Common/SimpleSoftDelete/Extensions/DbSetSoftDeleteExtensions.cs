using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cyclone.Common.SimpleSoftDelete.Extensions;

public static class DbSetSoftDeleteExtensions
{
    public static async Task SoftDeleteAsync<T>(
        this DbSet<T> set,
        Guid id,
        string? deletedBy = null,
        CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(set);
        var db = set.GetService<SimpleDbContext>()
                 ?? throw new InvalidOperationException("Не удалось получить DbContext из DbSet.");
        var entity = await db.FindAsync<T>(id, cancellationToken);
        if (entity == null) throw new ArgumentNullException($"Сущность с типом {typeof(T).Name} и id-- {id} не найдена");
        
        if (entity.IsDeleted) return;
        
        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.Now;
        
        await db.SaveChangesAsync(cancellationToken);;
    }
    
    public static Task<List<EntityDeletionInfo>> SoftDeleteCascadeAsync<T>(
        this DbSet<T> set,
        Guid id,
        string? deletedBy = null,
        bool useTransaction = true,
        CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(set);
        var current = set.GetService<ICurrentDbContext>();
        var db = current.Context ?? throw new InvalidOperationException("Не удалось получить DbContext из DbSet.");
        return SoftDeleteCascadeCoreAsync(db, set, id, deletedBy, useTransaction, cancellationToken);
    }

    public static Task<List<EntityDeletionInfo>> SoftDeleteCascadeAsync<T>(
        this DbSet<T> set,
        T rootEntity,
        string? deletedBy = null,
        bool useTransaction = true,
        CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
    ArgumentNullException.ThrowIfNull(set);
    ArgumentNullException.ThrowIfNull(rootEntity);
    var current = set.GetService<ICurrentDbContext>();
        var db = current.Context ?? throw new InvalidOperationException("Не удалось получить DbContext из DbSet.");
        return SoftDeleteCascadeCoreAsync(db, set, rootEntity, deletedBy, useTransaction, cancellationToken);
    }

    private static async Task<List<EntityDeletionInfo>> SoftDeleteCascadeCoreAsync<T>(
        DbContext db,
        DbSet<T> set,
        Guid id,
        string? deletedBy,
        bool useTransaction,
        CancellationToken cancellationToken)
        where T : BaseEntity
    {
        var root = await set.FindAsync([id], cancellationToken);
        if (root == null) throw new InvalidOperationException($"Сущность {typeof(T).Name} с id={id} не найдена.");
        return await SoftDeleteCascadeCoreAsync(db, set, root, deletedBy, useTransaction, cancellationToken);
    }

    private static async Task<List<EntityDeletionInfo>> SoftDeleteCascadeCoreAsync<T>(
        DbContext db,
        DbSet<T> set,
        BaseEntity rootEntity,
        string? deletedBy,
        bool useTransaction,
        CancellationToken cancellationToken)
        where T : BaseEntity
    {
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            if (useTransaction)
            {
                await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
                var cnt = await SoftDeleteRecursiveInternalAsync(db, rootEntity, deletedBy, [], cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return cnt;
            }
            else
            {
                var cnt = await SoftDeleteRecursiveInternalAsync(db, rootEntity, deletedBy, [], cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return cnt;
            }
        });
    }

    private static async Task<List<EntityDeletionInfo>> SoftDeleteRecursiveInternalAsync(
        DbContext db,
        object entityObj,
        string? deletedBy,
        HashSet<Guid> visited,
        CancellationToken cancellationToken)
    {
        if (entityObj is not BaseEntity be || !visited.Add(be.Id)) return [];

        var deleteEntityInfos = new List<EntityDeletionInfo>();

        if (!be.IsDeleted)
        {
            be.IsDeleted = true;
            be.DeletedAt = DateTime.UtcNow;
            be.DeletedBy = deletedBy;
            var entry = db.Entry(be);
            if (entry.State == EntityState.Detached) db.Attach(be);
            entry.State = EntityState.Modified;
            deleteEntityInfos.Add(new EntityDeletionInfo(entry.Entity, be.DeletedBy));
        }

        var policies = SoftDeletePolicyRegistry.GetPoliciesFor(entityObj.GetType());
        if (policies.Count == 0) return deleteEntityInfos;

        foreach (var nav in policies)
        {
            try
            {
                if (nav.IsCollection)
                {
                    var collEntry = db.Entry(entityObj).Collection(nav.NavigationName);
                    var query = collEntry.Query();
                    var list = await query
                        .OfType<BaseEntity>()
                        .IgnoreQueryFilters()
                        .ToListAsync(cancellationToken);

                    foreach (var childBase in list)
                    {
                        if (!nav.ChildType.IsInstanceOfType(childBase)) continue;
                        if (nav.Predicate != null && !nav.Predicate(childBase)) continue;
                        deleteEntityInfos.AddRange(await SoftDeleteRecursiveInternalAsync(db, childBase, deletedBy, visited, cancellationToken));
                    }
                }
                else
                {
                    var refEntry = db.Entry(entityObj).Reference(nav.NavigationName);
                    var childObj = await refEntry.Query()
                        .OfType<BaseEntity>()
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(cancellationToken);

                    if (childObj == null) continue;
                    if (!nav.ChildType.IsInstanceOfType(childObj)) continue;
                    if (nav.Predicate != null && !nav.Predicate(childObj)) continue;

                    deleteEntityInfos.AddRange(await SoftDeleteRecursiveInternalAsync(db, childObj, deletedBy, visited, cancellationToken));
                }
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Ошибка ");
            }
        }

        return deleteEntityInfos;
    }

    // ---------------- Restore ----------------

    public static Task<List<EntityDeletionInfo>> RestoreCascadeAsync<T>(
        this DbSet<T> set,
        Guid id,
        string? restoredBy = null,
        bool useTransaction = true,
        CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(set);
        var current = set.GetService<ICurrentDbContext>();
        var db = current.Context ?? throw new InvalidOperationException("Не удалось получить DbContext из DbSet.");
        return RestoreCascadeCoreAsync(db, set, id, restoredBy, useTransaction, cancellationToken);
    }

    public static Task<List<EntityDeletionInfo>> RestoreCascadeAsync<T>(
        this DbSet<T> set,
        T rootEntity,
        string? restoredBy = null,
        bool useTransaction = true,
        CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(rootEntity);
        var current = set.GetService<ICurrentDbContext>();
        var db = current.Context ?? throw new InvalidOperationException("Не удалось получить DbContext из DbSet.");
        return RestoreCascadeCoreAsync(db, set, rootEntity, restoredBy, useTransaction, cancellationToken);
    }

    private static async Task<List<EntityDeletionInfo>> RestoreCascadeCoreAsync<T>(
        DbContext db,
        DbSet<T> set,
        Guid id,
        string? restoredBy,
        bool useTransaction,
        CancellationToken cancellationToken)
        where T : BaseEntity
    {
        var root = await set.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (root == null) throw new InvalidOperationException($"Сущность {typeof(T).Name} с id={id} не найдена (даже через IgnoreQueryFilters).");
        return await RestoreCascadeCoreAsync(db, set, root, restoredBy, useTransaction, cancellationToken);
    }

    private static async Task<List<EntityDeletionInfo>> RestoreCascadeCoreAsync<T>(
        DbContext db,
        DbSet<T> set,
        BaseEntity rootEntity,
        string? restoredBy,
        bool useTransaction,
        CancellationToken cancellationToken)
        where T : BaseEntity
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            if (useTransaction)
            {
                await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
                var cnt = await RestoreRecursiveInternalAsync(db, rootEntity, restoredBy, [], cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return cnt;
            }
            else
            {
                var cnt = await RestoreRecursiveInternalAsync(db, rootEntity, restoredBy, [], cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return cnt;
            }
        });
    }

    private static async Task<List<EntityDeletionInfo>> RestoreRecursiveInternalAsync(
        DbContext db,
        object entityObj,
        string? restoredBy,
        HashSet<Guid> visited,
        CancellationToken cancellationToken)
    {
        if (entityObj is not BaseEntity be || !visited.Add(be.Id)) return [];

        List<EntityDeletionInfo> restored = [];
        if (be.IsDeleted)
        {
            be.IsDeleted = false;
            be.DeletedAt = null;
            be.DeletedBy = null;
            var entry = db.Entry(be);
            if (entry.State == EntityState.Detached) db.Attach(be);
            entry.State = EntityState.Modified;
            restored.Add(new EntityDeletionInfo(entry.Entity, restoredBy));
        }

        var policies = SoftDeletePolicyRegistry.GetPoliciesFor(entityObj.GetType());
        if (policies.Count == 0) return restored;

        foreach (var nav in policies)
        {
            if (nav.IsCollection)
            {
                var collEntry = db.Entry(entityObj).Collection(nav.NavigationName);
                var list = await collEntry.Query()
                    .OfType<BaseEntity>()
                    .IgnoreQueryFilters()
                    .ToListAsync(cancellationToken);

                foreach (var childBase in list)
                {
                    if (!nav.ChildType.IsInstanceOfType(childBase)) continue;
                    if (nav.Predicate != null && !nav.Predicate(childBase)) continue;
                    var res = await RestoreRecursiveInternalAsync(db, childBase, restoredBy, visited,
                        cancellationToken);
                    restored.AddRange(res);
                }
            }
            else
            {
                var reference = db.Entry(entityObj).Reference(nav.NavigationName);
                var child = await reference.Query()
                    .OfType<BaseEntity>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(cancellationToken);

                if (child == null) continue;
                if (!nav.ChildType.IsInstanceOfType(child)) continue;
                if (nav.Predicate != null && !nav.Predicate(child)) continue;
                var res = await RestoreRecursiveInternalAsync(db, child, restoredBy, visited, cancellationToken);
                restored.AddRange(res);
            }
        }

        return restored;
    }
}

