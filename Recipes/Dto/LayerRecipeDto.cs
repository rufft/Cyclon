using System.ComponentModel.DataAnnotations;
using Recipes.Models;

namespace Recipes.Dto;

public record CreateLayerRecipeDto
{
    public int LayerNumber { get; init; }
    public string? BatchRecipeId { get; init; }
    public string LayerTypeId { get; init; }
    public List<CreateLayerComponentDto> LayerComponents { get; init; }
    public string? MaskId { get; init; }
    public string? Description { get; init; }
}

public record UpdateLayerRecipeDto
{
    public int? LayerNumber { get; init; }
    public string? LayerRecipeId { get; init; }
    public string? LayerTypeId { get; init; }
    public List<LayerComponentDto>? LayerComponents { get; init; }
    public string? MaskId { get; init; }
    public string? Description { get; init; }
}