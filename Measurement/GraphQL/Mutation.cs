using Cyclone.Common.SimpleResponse;
using Measurement.Context;
using Measurement.Dto;
using Measurement.Models.MeasureTypes;
using Measurement.Services;

namespace Measurement.GraphQL;

public class Mutation(CieMeasureService cieMeasureService, MeasureDbContext db)
{

    public async Task<Response<CieMeasure>> CreateCieMeasureAsync(CreateCieMeasureDto input)
    {
        return await cieMeasureService.CreateAsync(input);
    }
}