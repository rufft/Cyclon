namespace Measurement.Models.MeasureTypes;

public class SpectraMeasure : Measure
{
    private SpectraMeasure() { }
    
    public SpectraMeasure(double[] weaveLength, double[] emissionIntensity, double[] current, double[] voltage)
    {
        ArgumentNullException.ThrowIfNull(weaveLength);
        ArgumentNullException.ThrowIfNull(emissionIntensity);
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(voltage);

        var len = weaveLength.Length;
        if (len == 0 ||
            emissionIntensity.Length != len ||
            current.Length != len ||
            voltage.Length != len)
        {
            throw new ArgumentException("Все четыре массива должны быть не пустыми и иметь одинаковую длину.");
        }

        WeaveLength = weaveLength;
        EmissionIntensity = emissionIntensity;
        Current = current;
        Voltage = voltage;
    }

    public double[] WeaveLength { get; init; }
    public double[] EmissionIntensity { get; init; }
    public double[] Current { get; init; }
    public double[] Voltage { get; init; }
}