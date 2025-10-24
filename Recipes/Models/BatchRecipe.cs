using Cyclone.Common.SimpleEntity;

namespace Recipes.Models;

public class BatchRecipe : BaseEntity
{
    private BatchRecipe() { }
    
    public BatchRecipe(Guid batchId, string substrate, List<LayerRecipe> layerRecipes, string? glassSerialNumber = null, string? description = null)
    {
        BatchId = batchId;
        Substrate = substrate;
        LayerRecipes = layerRecipes;
        GlassSerialNumber = glassSerialNumber;
        Description = description;
    }

    public Guid BatchId { get; init; }
    
    public string Substrate { get; set; }
    
    public string? GlassSerialNumber { get; set; }
    
    public List<LayerRecipe> LayerRecipes { get; set; }
    
}