using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleService;

namespace Cyclone.Common.SimpleSoftDelete;

public record DeleteEntityInfo(BaseEntity Entity, string? ServiceName);