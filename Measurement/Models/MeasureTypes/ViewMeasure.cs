namespace Measurement.Models.MeasureTypes;

public class ViewMeasure : Measure
{
    private ViewMeasure() { }
    
    public ViewMeasure(string? imagePath, bool? isDefected = null)
    {
        if (imagePath == null && isDefected == null)
            throw new ArgumentException("Изображение и дефектность не могут быть null одновременно");
        
        ImagePath = imagePath;
        IsDefected = isDefected;
    }

    public string? ImagePath { get; init; }
    public bool? IsDefected { get; init; }
}