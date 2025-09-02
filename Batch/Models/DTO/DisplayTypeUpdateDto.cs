using System.ComponentModel.DataAnnotations;

namespace Batch.Models.DTO;

public record DisplayTypeUpdateDto
{
    [ID]
    [Required(ErrorMessage = "Введите id")]
    public string? Id { get; init; }
    
    [StringLength(100)]
    public string? Name { get; init; }
    public SizeDto? Resolution { get; init; }
    public SizeDto? Format { get; init; }
    public SizeDto? ScreenSize { get; init; }
    
    [StringLength(500)]
    public string? Comment { get; init; }
}