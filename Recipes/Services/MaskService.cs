using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class MaskService(RecipeDbContext db, ILogger logger) : SimpleService<Mask, RecipeDbContext>(db, logger)
{
    private readonly RecipeDbContext _db = db;

    public async Task<Response<Mask>> CreateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите имя";

        if (_db.Masks.Any(x => x.Name == name))
            return "Маска с таким именем уже существует";
        var mask = new Mask(name, description);

        return await CreateAsync(mask);
    }
}