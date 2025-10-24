using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class MaterialService(RecipeDbContext db, ILogger logger) : SimpleService<Material, RecipeDbContext>(db, logger)
{
    private readonly RecipeDbContext _db = db;

    public async Task<Response<Material>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";
        if (_db.Materials.Any(x => x.Name == name))
            return "Материал с таким именем уже существует";
        var material = new Material(name, description);

        return await CreateAsync(material);
    }
}