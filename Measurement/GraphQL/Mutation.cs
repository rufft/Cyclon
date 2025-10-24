using System.ComponentModel.DataAnnotations;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleSoftDelete;
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
    public async Task<Response<CieMeasure>> CreateCieMeasureAsync(
        [Service] CieMeasureService cieMeasureService, CreateCieMeasureDto input) 
        => await cieMeasureService.CreateAsync(input);
    public async Task<Response<CieMeasure>> UpdateCieMeasureAsync(
        [Service] CieMeasureService cieMeasureService, UpdateCieMeasureDto input) 
        => await cieMeasureService.UpdateAsync(input);
    public async Task<Response<List<EntityDeletionInfo>>> DeleteCieMeasureAsync(
        [Service] CieMeasureService cieMeasureService, [Required] string? id)
        => await cieMeasureService.DeleteAsync(id);

    // Power
    public async Task<Response<PowerMeasure>> CreatePowerMeasureAsync(
        [Service] PowerMeasureService powerMeasureService, CreatePowerMeasureDto input) 
        => await powerMeasureService.CreateAsync(input);
    public async Task<Response<PowerMeasure>> UpdatePowerMeasureAsync(
        [Service] PowerMeasureService powerMeasureService, UpdatePowerMeasureDto input)
        => await powerMeasureService.UpdateAsync(input);
    public async Task<Response<List<EntityDeletionInfo>>> DeletePowerMeasureAsync(
        [Service] PowerMeasureService powerMeasureService, [Required] string? id) 
        => await powerMeasureService.DeleteAsync(id);

    // View
    public async Task<Response<ViewMeasure>> CreateViewMeasureAsync(
        [Service] ViewMeasureService viewMeasureService, CreateViewMeasureDto input)
    => await viewMeasureService.CreateAsync(input);
    public async Task<Response<List<EntityDeletionInfo>>> DeleteViewMeasureAsync(
        [Service] ViewMeasureService viewMeasureService, [Required] string? id) 
        => await viewMeasureService.DeleteAsync(id);
}