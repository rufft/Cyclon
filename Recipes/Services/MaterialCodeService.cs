using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class MaterialCodeService(RecipeDbContext db, ILogger logger) : SimpleService<MaterialCode, RecipeDbContext>(db, logger)
{
    private readonly RecipeDbContext _db = db;

    public async Task<Response<MaterialCode>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";
        
        if (_db.MaterialCodes.Any(x => x.Name == name))
            return "Код материала с таким именем уже существует";
        var materialCode = new MaterialCode(name, description);

        return await CreateAsync(materialCode);
    }
    
}