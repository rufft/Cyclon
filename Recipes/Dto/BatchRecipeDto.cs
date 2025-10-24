namespace Recipes.Dto;

public record CreateBatchRecipeDto
{
    public string BatchId { get; init; }
    public string Substrate { get; init; }
    public List<CreateLayerRecipeDto> LayerRecipeDtos { get; init; } 
    public string? GlassSerialNumber { get; init; }
    public string? Description { get; init; }
}

public record UpdateBatchRecipeDto
{
    public string BatchRecipeId { get; init; }
    public string? Substrate { get; init; }
    public List<UpdateLayerRecipeDto>? LayerRecipeDtos { get; init; }
    public string? GlassSerialNumber { get; init; }
    public string? Description { get; init; }
}