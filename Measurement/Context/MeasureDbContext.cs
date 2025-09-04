using Cyclone.Common.SimpleDatabase;
using Measurement.Models.MeasureTypes;
using Microsoft.EntityFrameworkCore;

namespace Measurement.Context;

public class MeasureDbContext(DbContextOptions<MeasureDbContext> options)
    : SimpleDbContext(options, typeof(CieMeasure).Assembly)
{
    public DbSet<CieMeasure> CieMeasures => Set<CieMeasure>();
    
}