using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Cyclone.Common.SimpleSoftDelete.Abstractions;

namespace Cyclone.Common.SimpleEntity;

public class BaseEntity : IIdentifier, ISoftDeletable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }
    
    public DateTime CreationTime { get; init; } = DateTime.Now;
    
    public DateTime ModificationTime { get; set; } = DateTime.Now;
    
    [GraphQLIgnore]
    [JsonIgnore]
    public bool IsDeleted { get; set; }
    
    [GraphQLIgnore]
    [JsonIgnore]
    public DateTime? DeletedAt { get; set; }
    
    [GraphQLIgnore]
    [JsonIgnore]
    public string? DeletedBy { get; set; }
    
    public string? Description { get; set; } 
}