using Batch.Context;
using Batch.Models;
using Batch.Models.Displays;
using Batch.Models.DTO;
using Batch.Services;
using Cyclone.Common.SimpleResponse;

namespace Batch.GraphQL;

public class Mutation
{
    private readonly BatchService _batchService;
    private readonly DisplayTypeService _displayTypeService;
    private readonly BatchDbContext _db;

    public Mutation(BatchService batchService, BatchDbContext db, DisplayTypeService displayTypeService)
    {
        _batchService = batchService;
        _db = db;
        _displayTypeService = displayTypeService;
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
    
    public async Task<Response<int>> SoftDeleteBatchAsync(string batchId)
    {
        return await _batchService.SoftDeleteBatchAsync(batchId);
    }
    
    public async Task<Response<int>> RestoreBatchAsync(string batchId)
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

    public async Task<Response<int>> SoftDeleteDisplayTypeAsync(string displayTypeId)
    {
        return await _displayTypeService.SoftDeleteDisplayTypeAsync(displayTypeId);
    }
    
    public async Task<Response<int>> RestoreDisplayTypeAsync(string displayTypeId)
    {
        return await _displayTypeService.RestoreDisplayTypeAsync(displayTypeId);
    }
    
}