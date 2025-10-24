using Batch.Context;
using Batch.Models;
using Batch.Models.Displays;
using Batch.Models.DTO;
using Batch.Services;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleSoftDelete;

namespace Batch.GraphQL;

public class Mutation
{
    private readonly BatchService _batchService;
    private readonly DisplayTypeService _displayTypeService;
    private readonly BatchDbContext _db;
    private readonly DisplayService _displayService;

    public Mutation(
        BatchService batchService,
        BatchDbContext db,
        DisplayTypeService displayTypeService,
        DisplayService displayService)
    {
        _batchService = batchService;
        _db = db;
        _displayTypeService = displayTypeService;
        _displayService = displayService;
    }
    
    // ==== Batch ====
    public async Task<Response<Models.Batch>> CreateBatchAsync(BatchCreateDto input)
    {
        return await _batchService.CreateBatchAsync(input);
    }

    public async Task<Response<Models.Batch>> UpdateBatchAsync(BatchUpdateDto input)
    {
        return await _batchService.UpdateBatchAsync(input);
    }
    
    public async Task<Response<List<EntityDeletionInfo>>> SoftDeleteBatchAsync(string batchId)
    {
        return await _batchService.SoftDeleteBatchAsync(batchId);
    }
    
    public async Task<Response<List<EntityDeletionInfo>>> RestoreBatchAsync(string batchId)
    {
        return await _batchService.RestoreBatchAsync(batchId);
    }

    // ==== DisplayType ====
    public async Task<Response<DisplayType>> CreateDisplayTypeAsync(DisplayTypeCreateDto input)
    {
        return await _displayTypeService.CreateDisplayTypeAsync(input);
    }
    
    public async Task<Response<DisplayType>> UpdateDisplayTypeAsync(DisplayTypeUpdateDto input)
    {
        return await _displayTypeService.UpdateDisplayTypeAsync(input);
    }

    public async Task<Response<List<EntityDeletionInfo>>> SoftDeleteDisplayTypeAsync(string displayTypeId)
    {
        return await _displayTypeService.SoftDeleteDisplayTypeAsync(displayTypeId);
    }
    
    public async Task<Response<List<EntityDeletionInfo>>> RestoreDisplayTypeAsync(string displayTypeId)
    {
        return await _displayTypeService.RestoreDisplayTypeAsync(displayTypeId);
    }
    
    // ==== Display ====
    public async Task<Response<List<EntityDeletionInfo>>> SoftDeleteDisplayAsync(string displayTypeId)
    {
        return await _displayService.SoftDeleteDisplayAsync(displayTypeId);
    }
    
    public async Task<Response<List<EntityDeletionInfo>>> RestoreDisplayAsync(string displayTypeId)
    {
        return await _displayService.RestoreDisplayAsync(displayTypeId);
    }
}