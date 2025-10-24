using Cyclone.Common.SimpleEntity;

namespace Recipes.Models;

public class LayerComponent : BaseEntity
{
    private LayerComponent() { }
    
    public LayerComponent(MaterialCode materialCode, Material material, string thickness)
    {
        MaterialCode = materialCode;
        Material = material;
        Thickness = thickness;
    }

    public MaterialCode MaterialCode { get; set; }
    public Material Material { get; set; }
    public string Thickness { get; set; }
    
    public LayerRecipe? LayerRecipe { get; set; }
}