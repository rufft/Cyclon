using Cyclone.Common.SimpleEntity;

namespace Recipes.Models;

public class LayerType : BaseEntity
{
    // pHIL, G-EML etc.
    private LayerType() { }
    
    public LayerType(string name, string? description)
    {
        Name = name;
        Description = description;
    }
    public string Name { get; set; }
    
}