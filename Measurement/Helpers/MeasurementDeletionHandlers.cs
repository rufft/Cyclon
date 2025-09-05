using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleSoftDelete;
using Cyclone.Common.SimpleSoftDelete.Extensions;
using Measurement.Models.MeasureTypes;
using Microsoft.EntityFrameworkCore;

namespace Measurement.Helpers;

public static class MeasurementDeletionHandlers
{
    public static async Task OnDisplayDeleteHandler(DeletionEvent ev, IServiceProvider sp, CancellationToken ct)
    {
        var dbContext = sp.GetService<SimpleDbContext>();
        var logger = sp.GetService<ILogger<DeletionListenerHostedService>>();
        if (dbContext is null) throw new NullReferenceException("DbContext не существует");

        var cieMeasures = dbContext.Set<CieMeasure>().Where(m => m.DisplayId == ev.EntityId);

        logger?.LogInformation($"Нашел ли измерения с Display id-- {ev.EntityId}: {cieMeasures.Any().ToString()}");

        foreach (var measure in cieMeasures)
        {
            await dbContext.Set<CieMeasure>().SoftDeleteCascadeAsync(measure, cancellationToken: ct);
        }
    }
}