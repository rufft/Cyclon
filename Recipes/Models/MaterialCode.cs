using Cyclone.Common.SimpleEntity;

namespace Recipes.Models;

public class MaterialCode : BaseEntity
{
    // PBF02, PPH05 etc.
    private MaterialCode() { }
    
    public MaterialCode(string name, string? description)
    {
        Name = name;
        Description = description;
    }
    public string Name { get; set; }
    
}