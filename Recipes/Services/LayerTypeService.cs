using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class LayerTypeService(RecipeDbContext db, ILogger logger) : SimpleService<LayerType, RecipeDbContext>(db, logger)
{
    public async Task<Response<LayerType>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";

        var layerType = new LayerType(name, description);

        return await CreateAsync(layerType);
    }

    public async Task<Response<List<DeleteEntityInfo>>> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Введите id";

        if (!Guid.TryParse(id, out var layerTypeId))
            return "Id не в формате Guid";
        
        var layerType = await db.FindAsync<LayerType>(layerTypeId);

        if (layerType == null)
            return $"Слоя с id-- {layerTypeId} не существует";
        
        return await SoftDeleteAsync(layerType);
    }
}