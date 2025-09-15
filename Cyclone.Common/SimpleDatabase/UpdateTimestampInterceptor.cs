using Cyclone.Common.SimpleEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cyclone.Common.SimpleDatabase;

public class UpdateTimestampInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not SimpleDbContext ctx)
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        var entries = ctx.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            entry.Entity.ModificationTime = now;
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}