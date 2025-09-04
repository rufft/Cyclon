using Batch.Context;
using Batch.Models.Displays;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Measurement.Context;
using Measurement.Models.MeasureTypes;
using Microsoft.EntityFrameworkCore;

namespace Measurement.GraphQL;

public class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<CieMeasure> CieMeasures([Service] MeasureDbContext database)
    {
        return database.CieMeasures.AsNoTracking();
    }
}