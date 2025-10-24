namespace Recipes.Dto;

public record CreateLayerComponentDto
{
    public string MaterialCodeId { get; init; }
    
    public string MaterialId { get; init; }
    
    public string Thickness { get; init; }
}

public record UpdateLayerComponentDto
{
    public string LayerComponentId { get; init; }
    
    public string? MaterialCodeId { get; init; }
    
    public string? MaterialId { get; init; }
    
    public string? Thickness { get; init; }
}

public record LayerComponentDto
{
    public string? LayerComponentId { get; init; }
    
    public string? MaterialCodeId { get; init; }
    
    public string? MaterialId { get; init; }
    
    public string? Thickness { get; init; }
}