using System.ComponentModel.DataAnnotations;

namespace Measurement.Dto;

public record CreateCieMeasureDto : MeasureDto
{
    [Required(ErrorMessage = "Введите cie x")]
    [Range(0, 1, ErrorMessage = "Cie x, y должен быть от 0 до 1")]
    public double? CieX { get; init; }
    
    [Required(ErrorMessage = "Введите cie y")]
    [Range(0, 1, ErrorMessage = "Cie x, y должен быть от 0 до 1")]
    public double? CieY { get; init; }
    
    [Required(ErrorMessage = "Введите Lv")]
    [Range(0, double.MaxValue, ErrorMessage = "Яркость должна быть больше 0")]
    public double? Lv { get; init; }
}

public record UpdateCieMeasureDto : MeasureDto
{
    [Range(0, 1, ErrorMessage = "Cie x, y должен быть от 0 до 1")]
    public double? CieX { get; init; }
    
    [Range(0, 1, ErrorMessage = "Cie x, y должен быть от 0 до 1")]
    public double? CieY { get; init; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Яркость должна быть больше 0")]
    public double? Lv { get; init; }
}
