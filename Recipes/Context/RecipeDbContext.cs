using Cyclone.Common.SimpleDatabase;
using Microsoft.EntityFrameworkCore;
using Recipes.Models;


namespace Recipes.Context;

public class RecipeDbContext(DbContextOptions<RecipeDbContext> options) 
    : SimpleDbContext(options, typeof(BatchRecipe).Assembly)

{
    public DbSet<Mask> Masks => Set<Mask>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<MaterialCode> MaterialCodes => Set<MaterialCode>();
    public DbSet<LayerRecipe> LayerRecipes => Set<LayerRecipe>();
    public DbSet<LayerComponent> LayerComponents => Set<LayerComponent>();
    public DbSet<BatchRecipe> BatchRecipes => Set<BatchRecipe>();
}