using Cyclone.Common.SimpleResponse;
using HotChocolate;
using Measurement.Context;
using Measurement.Dto;
using Measurement.Models.MeasureTypes;
using Measurement.Services;

namespace Measurement.GraphQL;

public class Mutation([Service] CieMeasureService cieMeasureService, [Service] MeasureDbContext db)
{ 
    // Cie
    public async Task<Response<CieMeasure>> CreateCieMeasureAsync(CreateCieMeasureDto input)
    {
        return await cieMeasureService.CreateAsync(input);
    }

    public async Task<Response<CieMeasure>> UpdateCieMeasureAsync(UpdateCieMeasureDto input)
    {
        return await cieMeasureService.UpdateAsync(input);
    }

    public async Task<Response<int>> DeleteCieMeasureAsync(string? cieId)
    {
        return await cieMeasureService.DeleteAsync(cieId);
    }
}