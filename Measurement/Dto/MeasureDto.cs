using System.ComponentModel.DataAnnotations;

namespace Measurement.Dto;

public abstract class MeasureDto
{
    [Required(ErrorMessage = "Введите id дисплея")]
    [StringLength(200)]
    public string DisplayId { get; init; }
}