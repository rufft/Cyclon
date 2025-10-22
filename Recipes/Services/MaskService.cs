using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class MaskService(RecipeDbContext db, ILogger logger) : SimpleService<Mask, RecipeDbContext>(db, logger)
{
    public async Task<Response<Mask>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";

        var mask = new Mask(name, description);

        return await CreateAsync(mask);
    }

    public async Task<Response<List<DeleteEntityInfo>>> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Введите id";

        if (!Guid.TryParse(id, out var maskId))
            return "Id не в формате Guid";
        
        var mask = await db.FindAsync<Mask>(maskId);

        if (mask == null)
            return $"Маски с id-- {maskId} не существует";
        
        return await SoftDeleteAsync(mask);
    }
}