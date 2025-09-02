using BatchClientNS;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Measurement.Context;
using Measurement.Dto;
using Measurement.Models.MeasureTypes;
using Microsoft.Extensions.Caching.Memory;

namespace Measurement.Services;

public class CieMeasureService(MeasureDbContext db) : SimpleService<CieMeasure, MeasureDbContext>(db)
{

    
}