using System.ComponentModel.DataAnnotations;

namespace Batch.Extensions.Validation;

public class SRangeAttribute : RangeAttribute
{
    public SRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
    {
    }

    public SRangeAttribute(int minimum, int maximum) : base(minimum, maximum)
    {
    }

    public SRangeAttribute(Type type, string minimum, string maximum) : base(type, minimum, maximum)
    {
    }
    
    public override string FormatErrorMessage(string name)
    {
        return $"Поле может быть только от { Minimum } до { Maximum }";
    }
}