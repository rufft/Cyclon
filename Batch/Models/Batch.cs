using Batch.Extensions;
using Batch.Models.Displays;
using Cyclone.Common.SimpleEntity;

namespace Batch.Models;

public class Batch : BaseEntity
{
    private Batch() { }
    
    public Batch(int number,
        string name,
        DisplayType displayDisplayType,
        DisplayColor color,
        Cover cover = Cover.All,
        string? description = null)
    {
        Number = number;
        Name = name;
        DisplayType = displayDisplayType;
        DisplayColor = color;
        Description = description;
        Cover = cover;
        
        Displays = _fillDisplays(displayDisplayType);
    }
    
    public int Number { get; set; }
    public string Name { get; set; }
    
    public Guid DisplayTypeId { get; private set; }
    public DisplayType DisplayType { get; init; }
    
    public DisplayColor DisplayColor { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Sputtered;
    
    public Cover Cover { get; set; } = Cover.All;
    
    public string? Description { get; set; }
    
    public List<Display> Displays { get; set; }

    private List<Display> _fillDisplays(DisplayType displayType)
    {
        var displays = new Display[displayType.AmountDisplays];
        var rows = displayType.AmountRows;
        var columns = displayType.AmountColumns;
        var format = displayType.CornersFormat;
        var counter = 0;
        
        for (var row = 1; row <= rows; row++)
        {
            var lShift = 0;
            var rShift = 0;

            if (format[0].Count >= row)
                lShift = format[0][row - 1];
            else if (format[2].Count > rows - row)
                lShift = format[2][rows - row];
            
            if (format[1].Count >= row)
                rShift = format[1][row - 1];
            else if (format[3].Count > rows - row)
                rShift = format[3][rows - row];
            
            for (var column = lShift + 1; column <= columns - rShift; column++)
            {
                var x = DisplayCounter.ConvertNumToDisplayCoordinates(column);
                var y = row.ToString();
                displays[counter] = new Display(displayType, new Coordinates(y, x), this, DisplayColor);
                counter++;
            }
        }
        
        return displays.ToList();
    }
    
    public override string ToString() => $"{Number} {DisplayType.Name} {Name}";
}

public enum BatchStatus
{
    Sputtered,
    Glued,
    Choped,
    Checked,
    PartiallyAssembled,
    Assembled
}

[Flags]
public enum Cover
{
    None = 0,
    
    First   = 1 << 0,
    Second  = 1 << 1,
    Third   = 1 << 2,
    Fourth  = 1 << 3,
    Fifth   = 1 << 4,
    Sixth   = 1 << 5,
    Seventh = 1 << 6,
    Eighth  = 1 << 7,
    Ninth   = 1 << 8,
    Tenth   = 1 << 9,
    Eleventh= 1 << 10,
    Twelfth = 1 << 11,
    Thirteenth = 1 << 12,
    Fourteenth  = 1 << 13,
    Fifteenth   = 1 << 14,
    Sixteenth   = 1 << 15,
    Seventeenth = 1 << 16,
    Eighteenth  = 1 << 17,
    Nineteenth  = 1 << 18,
    Twenteenth  = 1 << 19,
    
    All = (1 << 20) - 1
}