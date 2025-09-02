using Microsoft.EntityFrameworkCore;

namespace Measurement.Models.MeasureTypes;

public class CieMeasure : Measure
{
    private CieMeasure() { }
    
    public CieMeasure(Guid displayId, Cie cie, double lv)
    {
        DisplayId = displayId;
        Cie = cie;
        Lv = lv;
    }
    
    public Cie Cie { get; init; }
    public double Lv { get; init; }
}

[Owned]
public class Cie(double x, double y)
{
    public double X { get; init; } = x;
    public double Y { get; init; } = y;
}