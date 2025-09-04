using System.Text.Json;
using Batch.Models.Displays;
using Cyclone.Common.SimpleDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Batch.Context;

public class BatchDbContext(DbContextOptions<BatchDbContext> options)
    : SimpleDbContext(options, typeof(Models.Batch).Assembly)
{
    public DbSet<Models.Batch> Batches => Set<Models.Batch>();
    public DbSet<Display> Displays => Set<Display>();
    public DbSet<DisplayType> DisplayTypes => Set<DisplayType>();

    protected override void ConfigureDomainModel(ModelBuilder modelBuilder)
    {
        base.ConfigureDomainModel(modelBuilder);

        // JSON options
        var jsonOptions = new JsonSerializerOptions
            { PropertyNamingPolicy = null, WriteIndented = false };

        // Batch -> DisplayType
        modelBuilder.Entity<Models.Batch>(b =>
        {
            b.HasOne(x => x.DisplayType)
                .WithMany(dt => dt.Batches)
                .HasForeignKey(x => x.DisplayTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Displays)
                .WithOne(d => d.Batch)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Display -> DisplayType + Owned Coordinates
        modelBuilder.Entity<Display>(d =>
        {
            d.HasOne(x => x.DisplayType)
                .WithMany(dt => dt.Displays)
                .HasForeignKey(x => x.DisplayTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DisplayType owned types and CornersFormat JSON
        modelBuilder.Entity<DisplayType>(dt =>
        {
            var converter =
                new ValueConverter<List<List<int>>, string>(v =>
                    JsonSerializer.Serialize(v, jsonOptions), v =>
                    JsonSerializer.Deserialize<List<List<int>>>(v, jsonOptions) ?? new List<List<int>>());

            var comparer = new ValueComparer<List<List<int>>>(
                (a, b) => a!.Count == b!.Count && a.Select((row, i) => row.SequenceEqual(b[i])).All(x => x),
                v => v.Aggregate(17, (h, row) => row.Aggregate(h, HashCode.Combine)),
                v => v.Select(r => r.ToList()).ToList());

            dt.Property(x => x.CornersFormat)
                .HasConversion(converter)
                .Metadata.SetValueComparer(comparer);
        });
    }
}