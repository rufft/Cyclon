using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleService;

namespace Cyclone.Common.SimpleSoftDelete;

public record EntityDeletionInfo(BaseEntity Entity, string? ServiceName);