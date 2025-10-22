using Cyclone.Common.SimpleEntity;

namespace Recipes.Models;

public class Material : BaseEntity
{
    // HIL, Host1, BH etc.
    
    private Material() { }
    
    public Material(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; set; }
}