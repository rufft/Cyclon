using Batch.Context;
using Batch.Models.Displays;
using Microsoft.EntityFrameworkCore;

namespace Batch.GraphQL;

public class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DisplayType> DisplayTypes([Service] BatchDbContext database)
    {
        return database.DisplayTypes.AsNoTracking();
    }
    
    [UseFiltering]
    [UseSorting]
    public IQueryable<Display> Displays([Service] BatchDbContext database)
    {   
        return database.Displays
            .Include(d => d.Coordinates)
            .Include(d => d.DisplayType)
            .AsNoTracking();
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Models.Batch> Batches([Service] BatchDbContext database) =>
        database.Batches
            .Include(x => x.DisplayType)
            .Include(x => x.Displays)
            .ThenInclude(x => x.DisplayType)
            .AsNoTracking();

    public async Task<Display?> GetDisplayByIdAsync([Service] BatchDbContext database, Guid id) =>
        await database.Displays
            .Include(x => x.DisplayType)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Display?> GetDisplayByBatchAndCoordinatesAsync(
        [Service] BatchDbContext database,
        Guid batchId,
        string x,
        string y)
    {
        var batch = await database.Batches.AsNoTracking()
            .Include(batch => batch.Displays)
            .FirstOrDefaultAsync(batch => batch.Id == batchId);
        return batch?.Displays.FirstOrDefault(d => d.Coordinates.X == x && d.Coordinates.Y == y );
    }
    
    public async Task<Models.Batch?> GetBatchByIdAsync([Service] BatchDbContext database, Guid id) =>
        await database.Batches
            .Include(x => x.DisplayType)
            .Include(x => x.Displays)
            .FirstOrDefaultAsync(x => x.Id == id);

}