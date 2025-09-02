using Cyclone.Common.SimpleEntity;
using Microsoft.EntityFrameworkCore;

namespace Batch.Models.Displays;

public class Display : BaseEntity
{
    private Display() { }
    
    public Display(
        DisplayType displayType,
        Coordinates coordinates,
        Batch batch,
        DisplayColor color)
    {
        DisplayType = displayType;
        Coordinates = coordinates;
        Batch = batch;
        Color = color;
    }
    
    public Guid DisplayTypeId { get; private set; }
    public DisplayType DisplayType { get; init; }
     
    public Coordinates Coordinates { get; init; }
    
    public Guid BatchId { get; private set; }
    public Batch Batch { get; init; }
    
    public DisplayColor Color { get; init; }
    
    public string? OriginalPhotoPath { get; set; }
    
    public string? CroppedPhotoPath { get; set; }
    
    public string? Comment { get; set; }
}

[Owned]
public class Coordinates(string x, string y)
{
    public string X { get; init; } = x;
    public string Y { get; init; } = y;
}

public enum DisplayColor
{
    Red,
    Green,
    Blue,
    White,
    FullColor
}