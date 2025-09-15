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
    public CieMeasure(Guid displayId, double cieX, double cieY, double lv)
    {
        DisplayId = displayId;
        Cie = new Cie(cieX, cieY);
        Lv = lv;
    }
    
    public Cie Cie { get; init; }
    public double Lv { get; set; }
}

[Owned]
public class Cie(double x, double y)
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
}