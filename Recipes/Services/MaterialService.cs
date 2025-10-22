using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class MaterialService(RecipeDbContext db, ILogger logger) : SimpleService<Material, RecipeDbContext>(db, logger)
{
    public async Task<Response<Material>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";

        var material = new Material(name, description);

        return await CreateAsync(material);
    }

    public async Task<Response<List<DeleteEntityInfo>>> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Введите id";

        if (!Guid.TryParse(id, out var materialId))
            return "Id не в формате Guid";
        
        var material = await db.FindAsync<Material>(materialId);

        if (material == null)
            return $"Материала с id-- {materialId} не существует";
        
        return await SoftDeleteAsync(material);
    }
}