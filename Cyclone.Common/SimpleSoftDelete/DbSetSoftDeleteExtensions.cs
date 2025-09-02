using Cyclone.Common.SimpleEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cyclone.Common.SimpleSoftDelete;

public static class DbSetSoftDeleteExtensions
{
// Public API: soft delete by id
    public static Task<int> SoftDeleteCascadeAsync<T>(
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

    // Public API: soft delete by loaded entity
    public static Task<int> SoftDeleteCascadeAsync<T>(
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

    // Core: find entity by id, then delegate to entity-based method
    private static async Task<int> SoftDeleteCascadeCoreAsync<T>(
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

    // Core: main implementation
    private static async Task<int> SoftDeleteCascadeCoreAsync<T>(
        DbContext db,
        DbSet<T> set,
        BaseEntity rootEntity,
        string? deletedBy,
        bool useTransaction,
        CancellationToken cancellationToken)
        where T : BaseEntity
    {
        // Use execution strategy because of retry policies (Npgsql)
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            if (useTransaction)
            {
                await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
                var cnt = await SoftDeleteRecursiveInternalAsync(db, rootEntity, deletedBy, new HashSet<Guid>(), cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return cnt;
            }
            else
            {
                var cnt = await SoftDeleteRecursiveInternalAsync(db, rootEntity, deletedBy, new HashSet<Guid>(), cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return cnt;
            }
        });
    }

    // Core recursive worker (uses explicit policies)
    private static async Task<int> SoftDeleteRecursiveInternalAsync(
        DbContext db,
        object entityObj,
        string? deletedBy,
        HashSet<Guid> visited,
        CancellationToken cancellationToken)
    {
        if (entityObj is not BaseEntity be) return 0;
        if (!visited.Add(be.Id)) return 0;

        var marked = 0;

        // mark self if needed
        if (!be.IsDeleted)
        {
            be.IsDeleted = true;
            be.DeletedAt = DateTime.UtcNow;
            be.DeletedBy = deletedBy;
            var entry = db.Entry(be);
            if (entry.State == EntityState.Detached) db.Attach(be);
            entry.State = EntityState.Modified;
            marked++;
        }

        // get policies for this entity type
        var policies = SoftDeletePolicyRegistry.GetPoliciesFor(entityObj.GetType());
        if (policies.Count == 0) return marked;

        foreach (var nav in policies)
        {
            try
            {
                if (nav.IsCollection)
                {
                    // Collection navigation: use Query().IgnoreQueryFilters() to get deleted children too
                    var collEntry = db.Entry(entityObj).Collection(nav.NavigationName);
                    var query = collEntry.Query(); // non-generic IQueryable
                    // We will pull into memory as BaseEntity and then cast to child type and apply predicate (if any)
                    var list = await query
                        .OfType<BaseEntity>()            // now IQueryable<BaseEntity>
                        .IgnoreQueryFilters()
                        .ToListAsync(cancellationToken);

                    foreach (var childBase in list)
                    {
                        // check runtime type
                        if (!nav.ChildType.IsInstanceOfType(childBase)) continue;
                        // predicate check
                        if (nav.Predicate != null && !nav.Predicate(childBase)) continue;
                        marked += await SoftDeleteRecursiveInternalAsync(db, childBase, deletedBy, visited, cancellationToken);
                    }
                }
                else
                {
                    // Reference navigation
                    var refEntry = db.Entry(entityObj).Reference(nav.NavigationName);
                    var childObj = await refEntry.Query()
                        .OfType<BaseEntity>()
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(cancellationToken);

                    if (childObj == null) continue;
                    if (!nav.ChildType.IsInstanceOfType(childObj)) continue;
                    if (nav.Predicate != null && !nav.Predicate(childObj)) continue;

                    marked += await SoftDeleteRecursiveInternalAsync(db, childObj, deletedBy, visited, cancellationToken);
                }
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Ошибка ");
                continue;
            }
        }

        return marked;
    }

    // ---------------- Restore (mirror) ----------------

    public static Task<int> RestoreCascadeAsync<T>(
        this DbSet<T> set,
        Guid id,
        string? restoredBy = null,
        bool useTransaction = true,
        CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
        if (set == null) throw new ArgumentNullException(nameof(set));
        var current = set.GetService<ICurrentDbContext>();
        var db = current.Context ?? throw new InvalidOperationException("Не удалось получить DbContext из DbSet.");
        return RestoreCascadeCoreAsync(db, set, id, restoredBy, useTransaction, cancellationToken);
    }

    public static Task<int> RestoreCascadeAsync<T>(
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

    private static async Task<int> RestoreCascadeCoreAsync<T>(
        DbContext db,
        DbSet<T> set,
        Guid id,
        string? restoredBy,
        bool useTransaction,
        CancellationToken cancellationToken)
        where T : BaseEntity
    {
        // Fetch with IgnoreQueryFilters so we can restore root even if deleted
        var root = await set.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (root == null) throw new InvalidOperationException($"Сущность {typeof(T).Name} с id={id} не найдена (даже через IgnoreQueryFilters).");
        return await RestoreCascadeCoreAsync(db, set, root, restoredBy, useTransaction, cancellationToken);
    }

    private static async Task<int> RestoreCascadeCoreAsync<T>(
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
                var cnt = await RestoreRecursiveInternalAsync(db, rootEntity, restoredBy, new HashSet<Guid>(), cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return cnt;
            }
            else
            {
                var cnt = await RestoreRecursiveInternalAsync(db, rootEntity, restoredBy, new HashSet<Guid>(), cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return cnt;
            }
        });
    }

    private static async Task<int> RestoreRecursiveInternalAsync(
        DbContext db,
        object entityObj,
        string? restoredBy,
        HashSet<Guid> visited,
        CancellationToken cancellationToken)
    {
        if (entityObj is not BaseEntity be) return 0;
        if (!visited.Add(be.Id)) return 0;

        var restored = 0;
        if (be.IsDeleted)
        {
            be.IsDeleted = false;
            be.DeletedAt = null;
            be.DeletedBy = null;
            var entry = db.Entry(be);
            if (entry.State == EntityState.Detached) db.Attach(be);
            entry.State = EntityState.Modified;
            restored++;
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
                    restored += await RestoreRecursiveInternalAsync(db, childBase, restoredBy, visited, cancellationToken);
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
                restored += await RestoreRecursiveInternalAsync(db, child, restoredBy, visited, cancellationToken);
            }
        }

        return restored;
    }
}