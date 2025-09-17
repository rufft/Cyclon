using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Cyclone.Common.SimpleDatabase.FileSystem;

public sealed class FilesFeatureOptionsExtension : IDbContextOptionsExtension
{
    public bool Enabled { get; }

    public FilesFeatureOptionsExtension(bool enabled = true) => Enabled = enabled;

    // возможность создать копию с другим значением
    public FilesFeatureOptionsExtension WithEnabled(bool enabled) =>
        new(enabled);

    // === обязательные члены интерфейса ===
    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services) { /* ничего не добавляем */ }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo(FilesFeatureOptionsExtension ext) : DbContextOptionsExtensionInfo(ext)
    {
        public override bool IsDatabaseProvider => false;
        public override string LogFragment => ext.Enabled ? " FilesFeature(ON)" : " FilesFeature(OFF)";
        public override int GetServiceProviderHashCode() => ext.Enabled ? 1 : 0;
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) 
            => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
            debugInfo["FilesFeature:Enabled"] = ext.Enabled.ToString();
    }
}