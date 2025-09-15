using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace Measurement.Dto;

public abstract record MeasureDto
{
    [Required(ErrorMessage = "Введите id дисплея")]
    public string? DisplayId { get; init; }
}