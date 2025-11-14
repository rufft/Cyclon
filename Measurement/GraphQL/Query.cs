using HotChocolate;
using HotChocolate.Types;
using Measurement.Context;
using Measurement.Models.MeasureTypes;
using Microsoft.EntityFrameworkCore;

namespace Measurement.GraphQL;

public class Query
{
    //TODO убрать пагинацию
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<CieMeasure> CieMeasures([Service] MeasureDbContext database)
    {
        return database.CieMeasures
            .AsNoTracking()
            .OrderByDescending(m => m.ModificationTime);
    }
    
    public IQueryable<CieMeasure> LastBatchCieMeasures([Service] MeasureDbContext database, Guid batchId)
    {
        return database.CieMeasures.AsNoTracking()
            .Where(m => m.BatchId == batchId)
            .OrderByDescending(m => m.ModificationTime)
            .GroupBy(n => n.DisplayId)
            .Select(g => g.OrderByDescending(x => x.ModificationTime).First());
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PowerMeasure> PowerMeasures([Service] MeasureDbContext database)
    {
        return database.PowerMeasures.AsNoTracking().OrderByDescending(m => m.ModificationTime);
    }
    
        
    public IQueryable<PowerMeasure> LastBatchPowerMeasures([Service] MeasureDbContext database, Guid batchId)
    {
        return database.PowerMeasures.AsNoTracking()
            .Where(m => m.BatchId == batchId)
            .OrderByDescending(m => m.ModificationTime)
            .GroupBy(n => n.DisplayId)
            .Select(g => g.OrderByDescending(x => x.ModificationTime).First());
    }
}