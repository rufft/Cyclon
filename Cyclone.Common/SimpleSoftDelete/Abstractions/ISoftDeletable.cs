namespace Cyclone.Common.SimpleSoftDelete.Abstractions;

public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public string? DeletedBy { get; set; }
}