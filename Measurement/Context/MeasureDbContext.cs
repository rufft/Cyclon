using System.Text.Json;
using Cyclone.Common.SimpleDatabase;
using Measurement.Models.MeasureTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Measurement.Context;

public class MeasureDbContext(DbContextOptions<MeasureDbContext> options)
    : SimpleDbContext(options, typeof(CieMeasure).Assembly)
{
    public DbSet<CieMeasure> CieMeasures => Set<CieMeasure>();
    public DbSet<PowerMeasure> PowerMeasures => Set<PowerMeasure>();

    protected override void ConfigureDomainModel(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions
            { PropertyNamingPolicy = null, WriteIndented = false };

        modelBuilder.Entity<PowerMeasure>(dt =>
        {
            var listConverter = new ValueConverter<List<PowerPair>, string>(
                to => JsonSerializer.Serialize(to, jsonOptions),
                from => JsonSerializer.Deserialize<List<PowerPair>>(from, jsonOptions) ?? new List<PowerPair>());

            var listComparer = new ValueComparer<List<PowerPair>>(
                (a, b) => ReferenceEquals(a, b) || (a != null && b != null && a.SequenceEqual(b)),
                v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())));

            var singleConverter = new ValueConverter<PowerPair?, string>(
                to => JsonSerializer.Serialize(to, jsonOptions),
                from => JsonSerializer.Deserialize<PowerPair>(from, jsonOptions));

            var singleComparer = new ValueComparer<PowerPair>(
                (a, b) => Equals(a, b),
                v => v.GetHashCode(),
                v => v);
            dt.Property(x => x.PowerPairs)
                .HasConversion(listConverter)
                .Metadata.SetValueComparer(listComparer);

            dt.Property(x => x.ReversePowerPair)
                .HasConversion(singleConverter)
                .Metadata.SetValueComparer(singleComparer);
        });
    }
}