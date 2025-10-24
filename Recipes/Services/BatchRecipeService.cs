using Cyclone.Common.SimpleClient;
using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Microsoft.EntityFrameworkCore;
using Recipes.Context;
using Recipes.Dto;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class BatchRecipeService(
    RecipeDbContext db,
    ILogger logger,
    LayerRecipeService layerRecipeService,
    SimpleClient client)
    : SimpleService<BatchRecipe, RecipeDbContext>(db, logger)
{
    private readonly ILogger _logger = logger;
    private readonly RecipeDbContext _db = db;

    public async Task<Response<BatchRecipe>> CreateAsync(CreateBatchRecipeDto dto)
    {
        var parseResult = SimpleDbContext.TryParseStringToGuidResponse(dto.BatchId);
        if (parseResult.Failure)
            return Response<BatchRecipe>.Fail(parseResult.Message, parseResult.Errors);
        var batchId = parseResult.Data;

        var batchExists = await client.GetIdByIdAsync("Batch", batchId);
        if (batchExists.Failure)
            return Response<BatchRecipe>.Fail(batchExists.Message, batchExists.Errors);

        var layerDtos = dto.LayerRecipeDtos;

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var batchRecipe = new BatchRecipe(batchId, dto.Substrate, [], dto.GlassSerialNumber, dto.Description);
            var createBatchResult = await base.CreateAsync(batchRecipe);
            if (createBatchResult.Failure)
            {
                await tx.RollbackAsync();
                return Response<BatchRecipe>.Fail(createBatchResult.Message, createBatchResult.Errors);
            }

            var savedBatch = createBatchResult.Data!;

            foreach (var lrDtoWithBatch in layerDtos.Select(lrDto => lrDto with { BatchRecipeId = savedBatch.Id.ToString() }))
            {
                var createLayerResult = await layerRecipeService.CreateAsync(lrDtoWithBatch);
                if (!createLayerResult.Failure) continue;
                await tx.RollbackAsync();
                return Response<BatchRecipe>.Fail(createLayerResult.Message, createLayerResult.Errors);
            }

            await _db.Entry(savedBatch).Collection(b => b.LayerRecipes).LoadAsync();
            
            var all = savedBatch.LayerRecipes.OrderBy(l => l.LayerNumber ?? int.MaxValue).ThenBy(l => l.Id).ToList();
            for (var i = 0; i < all.Count; i++)
            {
                var expected = i + 1;
                if (all[i].LayerNumber != expected)
                    all[i].LayerNumber = expected;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            await _db.Entry(savedBatch).Collection(b => b.LayerRecipes).LoadAsync();
            return Response<BatchRecipe>.Ok(savedBatch);
        }
        catch (DbUpdateException dbEx)
        {
            await tx.RollbackAsync();
            _logger.Error(dbEx, "DB error while creating BatchRecipe");
            return Response<BatchRecipe>.Fail("Ошибка при сохранении: " + dbEx.Message);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.Error(ex, "Unexpected error while creating BatchRecipe");
            return Response<BatchRecipe>.Fail("Неожиданная ошибка: " + ex.Message);
        }
    }

    public async Task<Response<BatchRecipe>> UpdateAsync(UpdateBatchRecipeDto dto)
    {
        var batchRecipeResult = await _db.FindByStringAsync<BatchRecipe>(dto.BatchRecipeId);
        if (batchRecipeResult.Failure)
            return batchRecipeResult;
        var batchRecipe = batchRecipeResult.Data!;

        if (!string.IsNullOrWhiteSpace(dto.Substrate))
            batchRecipe.Substrate = dto.Substrate;
        if (!string.IsNullOrWhiteSpace(dto.GlassSerialNumber))
            batchRecipe.GlassSerialNumber = dto.GlassSerialNumber;
        if (!string.IsNullOrWhiteSpace(dto.Description))
            batchRecipe.Description = dto.Description;

        if (dto.LayerRecipeDtos == null || dto.LayerRecipeDtos.Count == 0)
        {
            await _db.SaveChangesAsync();
            return Response<BatchRecipe>.Ok(batchRecipe);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _db.Entry(batchRecipe).Collection(b => b.LayerRecipes).LoadAsync();
            var existingLayers = batchRecipe.LayerRecipes;
            var existingById = existingLayers.ToDictionary(l => l.Id, l => l);

            var offset = existingLayers.Count + dto.LayerRecipeDtos.Count + 5;

            var createdIdByDtoIndex = new Dictionary<int, Guid>();

            for (var idx = 0; idx < dto.LayerRecipeDtos.Count; idx++)
            {
                var layerDto = dto.LayerRecipeDtos[idx];

                var desiredNumber = layerDto.LayerNumber is > 0 
                    ? layerDto.LayerNumber.Value 
                    : idx + 1;

                if (!string.IsNullOrWhiteSpace(layerDto.LayerRecipeId) &&
                    Guid.TryParse(layerDto.LayerRecipeId, out var existingGuid) &&
                    existingById.TryGetValue(existingGuid, out var existingLayer))
                {
                    var updateDto = layerDto with 
                        { LayerRecipeId = existingLayer.Id.ToString(),
                            LayerNumber = (existingLayer.LayerNumber ?? desiredNumber) + offset,
                            LayerComponents = layerDto.LayerComponents?.Select(c => new LayerComponentDto
                    {
                        LayerComponentId = c.LayerComponentId,
                        MaterialId = c.MaterialId,
                        MaterialCodeId = c.MaterialCodeId,
                        Thickness = c.Thickness,
                    }).ToList() };

                    var updateResponse = await layerRecipeService.UpdateAsync(updateDto);
                    if (!updateResponse.Failure) continue;
                    await tx.RollbackAsync();
                    return Response<BatchRecipe>.Fail(updateResponse.Message, updateResponse.Errors);
                }

                var createDto = new CreateLayerRecipeDto
                {
                    LayerNumber = desiredNumber + offset,
                    BatchRecipeId = batchRecipe.Id.ToString(),
                    LayerTypeId = layerDto.LayerTypeId!,
                    LayerComponents = layerDto.LayerComponents?.Select(c => new CreateLayerComponentDto
                    {
                        MaterialId = c.MaterialId!,
                        MaterialCodeId = c.MaterialCodeId!,
                        Thickness = c.Thickness!,
                    }).ToList() ?? [],
                    MaskId = layerDto.MaskId,
                    Description = layerDto.Description
                };

                var createRes = await layerRecipeService.CreateAsync(createDto);
                if (createRes.Failure)
                {
                    await tx.RollbackAsync();
                    return Response<BatchRecipe>.Fail(createRes.Message, createRes.Errors);
                }

                var created = createRes.Data!;
                createdIdByDtoIndex[idx] = created.Id;

                await _db.Entry(batchRecipe).Collection(b => b.LayerRecipes).LoadAsync();
            }

            await _db.Entry(batchRecipe).Collection(b => b.LayerRecipes).LoadAsync();
            var allLayers = batchRecipe.LayerRecipes.OrderBy(l => l.LayerNumber 
                                                                  ?? int.MaxValue).ThenBy(l => l.Id).ToList();
            var layersById = allLayers.ToDictionary(l => l.Id, l => l);

            var finalOrderedIds = new List<Guid>(dto.LayerRecipeDtos.Count);
            for (var idx = 0; idx < dto.LayerRecipeDtos.Count; idx++)
            {
                var layerDto = dto.LayerRecipeDtos[idx];
                if (!string.IsNullOrWhiteSpace(layerDto.LayerRecipeId)
                    && Guid.TryParse(layerDto.LayerRecipeId, out var gid) && layersById.ContainsKey(gid))
                    finalOrderedIds.Add(gid);
                else if (createdIdByDtoIndex.TryGetValue(idx, out var createdId)) finalOrderedIds.Add(createdId);
            }

            foreach (var l in allLayers.Where(l => !finalOrderedIds.Contains(l.Id)))
            {
                finalOrderedIds.Add(l.Id);
            }

            const int bigOffset = 1_000_000;
            await _db.LayerRecipes
                .Where(l => l.BatchRecipe.Id == batchRecipe.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    l => l.LayerNumber,
                    l => (l.LayerNumber ?? 0) + bigOffset));
            
            for (var i = 0; i < finalOrderedIds.Count; i++)
            {
                var id = finalOrderedIds[i];
                var desired = i + 1;
                await _db.LayerRecipes
                    .Where(l => l.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(l => l.LayerNumber, _ => desired));
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            await _db.Entry(batchRecipe).Collection(b => b.LayerRecipes).LoadAsync();
            return batchRecipe;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.Error(ex, "Error while updating BatchRecipe {BatchRecipeId}", dto.BatchRecipeId);
            return "Неожиданная ошибка: " + ex.Message;
        }
    }

    public async Task<Response<List<EntityDeletionInfo>>> DeleteLayerAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Введите id";
        if (!Guid.TryParse(id, out var layerRecipeId))
            return "Id не в формате Guid";
        
        var layer = await _db.LayerRecipes
            .Include(layerRecipe => layerRecipe.BatchRecipe)
            .FirstOrDefaultAsync(l => l.Id == layerRecipeId);
        if (layer == null)
            return $"LayerRecipe с id-- {layerRecipeId} не существует";
        
        var batchRecipe = layer.BatchRecipe;
        Guid? batchRecipeId = batchRecipe.Id;

        var deleteResult = await layerRecipeService.DeleteAsync(id);
        if (deleteResult.Failure)
            return Response<List<EntityDeletionInfo>>.Fail(deleteResult.Message, deleteResult.Errors);

        if (batchRecipeId.Value == Guid.Empty) return deleteResult;
        
        try
        {
            var batchRecipeResult = await _db.FindByStringAsync<BatchRecipe>(batchRecipeId.Value.ToString());
            if (!batchRecipeResult.Failure)
            {
                var batchEntity = batchRecipeResult.Data!;
                await _db.Entry(batchEntity).Collection(b => b.LayerRecipes).LoadAsync();

                var layers = batchEntity.LayerRecipes
                    .OrderBy(l => l.LayerNumber ?? int.MaxValue)
                    .ThenBy(l => l.Id)
                    .ToList();

                for (var i = 0; i < layers.Count; i++)
                {
                    var expected = i + 1;
                    if (layers[i].LayerNumber != expected)
                        layers[i].LayerNumber = expected;
                }

                await _db.SaveChangesAsync();
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.Error(dbEx, "Ошибка при переиндексации LayerNumber после удаления слоя {LayerId}", id);
            return "Удаление выполнено, но не удалось обновить номера слоёв: " + dbEx.Message;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка при переиндексации слоёв после удаления {LayerId}", id);
            return "Удаление выполнено, но произошла ошибка при обновлении порядковых номеров: " + ex.Message;
        }

        return deleteResult;
    }

}