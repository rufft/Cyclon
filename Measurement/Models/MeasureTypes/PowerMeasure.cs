using Measurement.Models.MeasureTypes;

namespace Measurement.Models.MeasureTypes;

public class PowerMeasure : Measure
{
    private PowerMeasure() { }

    public PowerChannels Channels =>
        PowerPairs.Count switch
        {
            1 => PowerChannels.OneChannel,
            3 => PowerChannels.ThreeChannel,
            _ => throw new ArgumentOutOfRangeException(nameof(PowerPair), "Каналов может быть либо 1, либо 3")
        };

    public List<PowerPair> PowerPairs { get; internal set; }
    
    public PowerPair? ReversePowerPair { get; internal set; }

    public PowerMeasure(Guid displayId, List<PowerPair> powerPairs, PowerPair? reversePowerPair = null)
    {
        DisplayId = displayId;
        PowerPairs = PowerPair.IsInCorrectChanel(powerPairs)
            ? powerPairs
            : throw new ArgumentOutOfRangeException(nameof(powerPairs), "Каналов может быть либо 1, либо 3");
        ReversePowerPair = reversePowerPair;
    }
}

public record PowerPair(double Current, double? Voltage = null)
{
    public static bool IsInCorrectChanel(List<PowerPair> powerPairs) => powerPairs.Count is 3 or 1;
}

public enum PowerChannels
{
    OneChannel = 1,
    ThreeChannel = 3
}