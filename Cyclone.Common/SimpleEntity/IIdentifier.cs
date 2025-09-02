namespace Cyclone.Common.SimpleEntity;

public interface IIdentifier
{
    public Guid Id { get; init; }
    public DateTime CreationTime { get; init; }
    
    public DateTime ModificationTime { get; set; }
}