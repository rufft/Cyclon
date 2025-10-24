using Batch.Context;
using Batch.Models.Displays;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Batch.Services;

public class DisplayService(BatchDbContext db, ILogger logger) : SimpleService<Display, BatchDbContext>(db, logger)
{
    public async Task<Response<List<EntityDeletionInfo>>> SoftDeleteDisplayAsync(string id)
    {
        if (!Guid.TryParse(id, out var displayId))
            return "Id имеет неверный формат GUID.";
        
        var display = await Db.Displays.FindAsync(displayId);
        if (display == null)
            return $"Дисплей с id = {id} не найден";
        
        return await SoftDeleteAsync(display);
    }

    public async Task<Response<List<EntityDeletionInfo>>> RestoreDisplayAsync(string id)
    {
        if (!Guid.TryParse(id, out var displayId))
            return "Id имеет неверный формат GUID.";
        
        var display = await Db.Displays
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == displayId);
        if (display == null)
            return $"Дисплей с id = {id} не найден";
        
        return await RestoreAsync(display);
    }
}