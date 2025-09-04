using System.ComponentModel.DataAnnotations;
using Batch.Extensions.Validation;
using Batch.Models.Displays;
using static Batch.Extensions.Validation.ValidationConstants;

namespace Batch.Models.DTO;

public record BatchCreateDto
{
    [Required(ErrorMessage = "Введите номер партии!")]
    [SRange(BATCH_NUM_MIN, BATCH_NUM_MAX)]
    public int? Number { get; init; }
    
    [Required(ErrorMessage = "Введите имя партии!")]
    [FieldStringLength(BATCH_NAME_MAX)]
    public string? Name { get; init; }

    [Required(ErrorMessage = "Введите Id типа дисплея!")]
    [FieldStringLength(ID_MAX_LEN)]
    public Guid? DisplayTypeId { get; init; }
    
    [Required(ErrorMessage = "Введите цвет дисплеев!")]
    [FieldStringLength(BATCH_COLOR_MAX)]
    public string? Color { get; init; }
    
    public int[]? Cover { get; init; }
    
    [FieldStringLength(BATCH_DESC_MAX)]
    public string? Description { get; init; }
}