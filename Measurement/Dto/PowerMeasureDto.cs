using System.ComponentModel.DataAnnotations;
using Measurement.Models.MeasureTypes;

namespace Measurement.Dto;

public record CreatePowerMeasureDto : MeasureDto
{
    [Required]
    public string? DisplayId { get; init; }
    
    [Required]
    public PowerPair[]? PowerPairs { get; init; }
    
    public PowerPair? ReversePowerPairs { get; init; }
}

public record UpdatePowerMeasureDto
{
    [Required]
    public string? Id { get; init; }
    public PowerPair[]? PowerPairs { get; init; }
    
    public PowerPair? ReversePowerPairs { get; init; }
}