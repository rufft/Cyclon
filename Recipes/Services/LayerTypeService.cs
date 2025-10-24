using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class LayerTypeService(RecipeDbContext db, ILogger logger) : SimpleService<LayerType, RecipeDbContext>(db, logger)
{
    private readonly RecipeDbContext _db = db;

    public async Task<Response<LayerType>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";
        
        if (_db.LayerTypes.Any(x => x.Name == name))
            return "Тип слоя с таким именем уже существует";
        var layerType = new LayerType(name, description);

        return await CreateAsync(layerType);
    }
    
}