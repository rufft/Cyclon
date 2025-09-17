using Cyclone.Common.SimpleEntity;

namespace Measurement.Models.MeasureTypes;

public abstract class Measure : BaseEntity
{
    public Guid DisplayId { get; protected init; }
}