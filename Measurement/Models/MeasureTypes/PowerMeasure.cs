using Measurement.Models.MeasureTypes;

namespace Measurement.Models.MeasureTypes;

public class PowerMeasure : Measure
{
    private PowerMeasure() { }
    
    public PowerChannelAvailable ChannelAvailable { get; private set; }

    public PowerMeasure(PowerChannelAvailable channelAvailable, double[] current, double voltage)
    {
        ChannelAvailable = channelAvailable;
        Voltage = voltage;

        ArgumentNullException.ThrowIfNull(current);

        switch (ChannelAvailable)
        {
            case PowerChannelAvailable.OneChannel:
                if (current.Length != 1)
                    throw new ArgumentException("Для одно-канального измерения требуется ровно одно значение current.", nameof(current));
                break;

            case PowerChannelAvailable.ThreeChannel:
                if (current.Length != 1 && current.Length != 3)
                    throw new ArgumentException("Для трёх-канального измерения допустимо 1 или 3 значения current.", nameof(current));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(channelAvailable), channelAvailable, "Неизвестный режим каналов.");
        }
        
        ChannelAvailable = channelAvailable;

        Current = (double[])current.Clone();
    }
    
    public double Voltage { get; private set; }

    public double[] Current { get; private set; }
    
}
public enum PowerChannelAvailable
{
    OneChannel = 1,
    ThreeChannel = 3
}