using System.Text.Json;
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
    
    public DbSet<LayerType> LayerTypes => Set<LayerType>();

    protected override void ConfigureDomainModel(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions
            { PropertyNamingPolicy = null, WriteIndented = false };
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BatchRecipe>()
            .Navigation(l => l.LayerRecipes)
            .AutoInclude();
        
        modelBuilder.Entity<LayerRecipe>()
            .Navigation(l => l.LayerComponents)
            .AutoInclude();
        modelBuilder.Entity<LayerRecipe>()
            .Navigation(l => l.Mask)
            .AutoInclude();
        modelBuilder.Entity<LayerRecipe>()
            .Navigation(l => l.LayerType)
            .AutoInclude();

        modelBuilder.Entity<LayerComponent>()
            .Navigation(c => c.Material)
            .AutoInclude();
        modelBuilder.Entity<LayerComponent>()
            .Navigation(c => c.MaterialCode)
            .AutoInclude();
        
    }
}