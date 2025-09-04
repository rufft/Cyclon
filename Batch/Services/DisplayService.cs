using Batch.Context;
using Batch.Models.Displays;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Microsoft.EntityFrameworkCore;

namespace Batch.Services;

public class DisplayService(BatchDbContext db) : SimpleService<Display, BatchDbContext>(db)
{
    public async Task<Response<int>> SoftDeleteDisplayAsync(string id)
    {
        if (!Guid.TryParse(id, out var displayId))
            return "Id имеет неверный формат GUID.";
        
        var display = await Db.Displays.FindAsync(displayId);
        if (display == null)
            return $"Дисплей с id = {id} не найден";
        
        return await SoftDeleteAsync(display);
    }

    public async Task<Response<int>> RestoreDisplayAsync(string id)
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