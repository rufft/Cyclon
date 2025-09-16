using Cyclone.Common.SimpleResponse;
using HotChocolate;
using HotChocolate.Types.Composite;
using Measurement.Context;
using Measurement.Dto;
using Measurement.Models.MeasureTypes;
using Measurement.Services;

namespace Measurement.GraphQL;

public class Mutation([Service] MeasureDbContext db)
{ 
    // Cie
    public async Task<Response<CieMeasure>> CreateCieMeasureAsync([Service] CieMeasureService cieMeasureService, CreateCieMeasureDto input)
    {
        return await cieMeasureService.CreateAsync(input);
    }

    public async Task<Response<CieMeasure>> UpdateCieMeasureAsync([Service] CieMeasureService cieMeasureService, UpdateCieMeasureDto input)
    {
        return await cieMeasureService.UpdateAsync(input);
    }

    public async Task<Response<int>> DeleteCieMeasureAsync([Service] CieMeasureService cieMeasureService, [Require] string? id)
    {
        return await cieMeasureService.DeleteAsync(id);
    }
    
    // Power
    public async Task<Response<PowerMeasure>> CreatePowerMeasureAsync([Service] PowerMeasureService powerMeasureService, CreatePowerMeasureDto input)
    {
        return await powerMeasureService.CreateAsync(input);
    }

    public async Task<Response<PowerMeasure>> UpdatePowerMeasureAsync([Service] PowerMeasureService powerMeasureService, UpdatePowerMeasureDto input)
    {
        return await powerMeasureService.UpdateAsync(input);
    }
    
    public async Task<Response<int>> DeletePowerMeasureAsync([Service] PowerMeasureService powerMeasureService, [Require] string? id)
    {
        return await powerMeasureService.DeleteAsync(id);
    }
}