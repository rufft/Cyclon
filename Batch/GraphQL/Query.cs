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
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Display> Displays([Service] BatchDbContext database)
    {
        return database.Displays.AsNoTracking();
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Models.Batch> Batches([Service] BatchDbContext database)
    {
        return database.Batches
            .Include(x => x.DisplayType)
            .Include(x => x.Displays)
            .ThenInclude(x => x.DisplayType)
            .AsNoTracking();
    }

}