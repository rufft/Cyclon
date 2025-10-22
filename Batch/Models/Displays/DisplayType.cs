using Cyclone.Common.SimpleEntity;
using Microsoft.EntityFrameworkCore;

namespace Batch.Models.Displays;

public class DisplayType : BaseEntity
{
    private DisplayType() { }
    
    public DisplayType(
        string name,
        Size resolution,
        Size format,
        Size screenSize,
        int amountRows,
        int amountColumns,
        List<List<int>> cornersFormat, 
        string? description = null)
    {
        Name = name;
        Resolution = resolution;
        Format = format;
        ScreenSize = screenSize;
        AmountRows = amountRows;
        AmountColumns = amountColumns;
        CornersFormat = cornersFormat;
        Description = description;

        var lostDisplays = cornersFormat.SelectMany(row => row).Sum();

        AmountDisplays = amountRows * amountColumns - lostDisplays;
    }
    
    public string Name { get; set; }
    
    public int AmountRows { get; set; }
    
    public int AmountColumns { get; set; }
    
    public int AmountDisplays { get; set; }

    public List<List<int>> CornersFormat { get; set; }

    public Size Resolution { get; set; }
    
    public Size Format { get; set; }
    
    public Size ScreenSize { get; set; }
    
    public string? Comment { get; set; }
    
    public List<Batch> Batches { get; set; }
    
    public List<Display> Displays { get; set; }
}

[Owned]
public class Size(double width, double height)
{
    public double Width { get; init; } = width;
    public double Height { get; init; } = height;

    public override string ToString() => $"{Width}x{Height}";
}