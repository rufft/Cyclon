using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cyclone.Common.SimpleDatabase.FileSystem;

public static class FilesFeatureOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseUploadedFiles(
        this DbContextOptionsBuilder builder, bool enabled = true)
    {
        IDbContextOptionsBuilderInfrastructure infrastructure = builder;
        
        var existing = builder.Options.FindExtension<FilesFeatureOptionsExtension>();
        var updated = (existing ?? new FilesFeatureOptionsExtension()).WithEnabled(enabled);

        infrastructure.AddOrUpdateExtension(updated);
        return builder;
    }
}