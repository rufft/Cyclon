using System.ComponentModel.DataAnnotations;
using Batch.Extensions.Validation;

namespace Batch.Models.DTO;

public record SizeDto
{
    [Required]
    public double? Width { get; init; }
    
    [Required]
    public double? Height { get; init; }
}

public record DisplayTypeCreateDto
{
    [Required(ErrorMessage = "Введите имя типа дисплея!!!")]
    [StringLength(2, ErrorMessage = "Тестим ыы")]
    public string? Name { get; init; } = null!;

    [Required]
    public SizeDto? Resolution { get; init; } = null!;
    
    [Required]
    public SizeDto? Format { get; init; } = null!;
    
    [Required]
    public SizeDto? ScreenSize { get; init; } = null!;

    [Required]
    [Range(1, 50, ErrorMessage = "Значение должно быть в диапозоне от 1 до 50")]
    public int? AmountRows { get; init; }

    [Required]
    [Range(1, 50, ErrorMessage = "Значение должно быть в диапозоне от 1 до 50")]
    public int? AmountColumns { get; init; }
    
    [Required]
    [CornerFormat]
    public List<List<int>>? CornersFormat { get; init; }

    [StringLength(500)]
    public string? Comment { get; init; }
}