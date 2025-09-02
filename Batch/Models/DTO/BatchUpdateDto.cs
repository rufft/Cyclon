using System.ComponentModel.DataAnnotations;
using Batch.Extensions.Validation;

namespace Batch.Models.DTO;

public record BatchUpdateDto
{
    [ID]
    [Required(ErrorMessage = "Введите Id партии!")]
    [FieldStringLength(ValidationConstants.ID_MAX_LEN)]
    public string Id { get; init; }
    
    [SRange(ValidationConstants.BATCH_NUM_MIN, ValidationConstants.BATCH_NUM_MAX)]
    public int? Number { get; init; }
    
    [FieldStringLength(ValidationConstants.BATCH_NAME_MAX)]
    public string? Name { get; init; }
    
    [FieldStringLength(ValidationConstants.BATCH_COLOR_MAX)]
    public string? Color { get; init; }
    
    [FieldStringLength(ValidationConstants.BATCH_STATUS_MAX)]
    public string? Status { get; init; }
    
    [FieldStringLength(ValidationConstants.BATCH_DESC_MAX)]
    public string? Description { get; init; }
}