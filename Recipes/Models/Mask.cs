using Cyclone.Common.SimpleEntity;

namespace Recipes.Models;

public class Mask : BaseEntity
{
    
    // Organic, TFE etc.
    private Mask() { }
    
    public Mask(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
    public string Name { get; set; }
}