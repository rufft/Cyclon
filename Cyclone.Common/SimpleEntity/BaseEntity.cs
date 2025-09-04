using System.ComponentModel.DataAnnotations.Schema;
using Cyclone.Common.SimpleSoftDelete;
using Cyclone.Common.SimpleSoftDelete.Abstractions;

namespace Cyclone.Common.SimpleEntity;

public class BaseEntity : IIdentifier, ISoftDeletable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }
    
    public DateTime CreationTime { get; init; } = DateTime.Now;
    
    public DateTime ModificationTime { get; set; } = DateTime.Now;
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; } = null;
    
    public string? DeletedBy { get; set; } = string.Empty;
}