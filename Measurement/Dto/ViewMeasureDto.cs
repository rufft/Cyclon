using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace Measurement.Dto;

public record CreateViewMeasureDto : MeasureDto
{
    public bool IsDefected { get; init; }
    
    [GraphQLType(typeof(UploadType))]
    public IFile? ImageFile { get; init; }
}

public record UpdateViewMeasureDto
{
    public bool? IsDefected { get; init; }
    
    public IFile? ImageFile { get; init; }
}