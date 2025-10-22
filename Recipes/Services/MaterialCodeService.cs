using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class MaterialCodeService(RecipeDbContext db, ILogger logger) : SimpleService<MaterialCode, RecipeDbContext>(db, logger)
{
    public async Task<Response<MaterialCode>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";

        var materialCode = new MaterialCode(name, description);

        return await CreateAsync(materialCode);
    }

    public async Task<Response<List<DeleteEntityInfo>>> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Введите id";

        if (!Guid.TryParse(id, out var materialCodeId))
            return "Id не в формате Guid";
        
        var materialCode = await db.FindAsync<MaterialCode>(materialCodeId);

        if (materialCode == null)
            return $"Код материала с id-- {materialCodeId} не существует";
        
        return await SoftDeleteAsync(materialCode);
    }
}