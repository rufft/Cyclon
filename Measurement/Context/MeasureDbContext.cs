using Cyclone.Common.SimpleDatabase;
using Microsoft.EntityFrameworkCore;

namespace Measurement.Context;

public class MeasureDbContext(DbContextOptions<MeasureDbContext> options) : SimpleDbContext(options);