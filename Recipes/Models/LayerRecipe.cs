using Cyclone.Common.SimpleEntity;

namespace Recipes.Models;

public class LayerRecipe : BaseEntity
{

    private LayerRecipe() { }
    
    public LayerRecipe(int layerNumber, BatchRecipe batchRecipe , LayerType layerType, List<LayerComponent> layerComponents, Mask? mask = null, string? description = null)
    {
        BatchRecipe = batchRecipe;
        LayerType = layerType;
        Mask = mask;
        LayerComponents = layerComponents;
        LayerNumber = layerNumber;
        Description = description;
    }
    
    public int? LayerNumber { get; set; }
    
    public LayerType LayerType { get; set; }
    public Mask? Mask { get; set; }
    public List<LayerComponent> LayerComponents { get; set; }
    
    public BatchRecipe BatchRecipe { get; init; }
}