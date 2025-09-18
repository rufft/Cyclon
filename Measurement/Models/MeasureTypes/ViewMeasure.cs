using Cyclone.Common.SimpleDatabase.FileSystem;

namespace Measurement.Models.MeasureTypes;

public class ViewMeasure : Measure
{
    private ViewMeasure() { }
    
    public ViewMeasure(Guid displayId, bool isDefected, UploadedFile? originalImage = null, UploadedFile? compresedImage = null)
    {
        DisplayId = displayId;
        IsDefected = isDefected;
        OriginalImage = originalImage;
        CompresedImage = compresedImage;
    }
    
    private UploadedFile? _originalImage;
    public UploadedFile? OriginalImage
    {
        get => _originalImage;
        set
        {
            if (value is null)
            {
                _originalImage = null;
                return;
            }   
            if (UploadedFile.GetCategory(value.FileType) is not FileCategory.Image)
                throw new InvalidDataException(nameof(value));
            _originalImage = value;
        }
    }

    private UploadedFile? _compresedImage;
    public UploadedFile? CompresedImage
    {
        get => _compresedImage;
        set
        {
            if (value is null)
            {
                _compresedImage = null;
                return;
            }   
            if (UploadedFile.GetCategory(value.FileType) is not FileCategory.Image)
                throw new InvalidDataException(nameof(value));
            _compresedImage = value;
        }
    }
    
    public bool IsDefected { get; set; }
}