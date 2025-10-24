using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Recipes.Context;
using Recipes.Models;

namespace Recipes.GraphQL;

public class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<BatchRecipe> BatchRecipes([Service] RecipeDbContext database)
    {
        return database.BatchRecipes.AsNoTracking();
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Material> Materials([Service] RecipeDbContext database)
    {
        return database.Materials.AsNoTracking();
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<MaterialCode> MaterialCodes([Service] RecipeDbContext database)
    {
        return database.MaterialCodes.AsNoTracking();
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<LayerType> LayerTypes([Service] RecipeDbContext database)
    {
        return database.LayerTypes.AsNoTracking();
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Mask> Masks([Service] RecipeDbContext database)
    {
        return database.Masks.AsNoTracking();
    }
}