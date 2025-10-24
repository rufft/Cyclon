using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Dto;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class LayerRecipeService(RecipeDbContext db, ILogger logger, LayerComponentService layerComponentService)
    : SimpleService<LayerRecipe, RecipeDbContext>(db, logger)
{
    private readonly RecipeDbContext _db = db;

    public async Task<Response<LayerRecipe>> CreateAsync(CreateLayerRecipeDto dto)
    {
        if (dto.LayerNumber <= 0)
            return "Номер слоя должен быть положительным";
        
        var batchRecipeResult = await _db.FindByStringAsync<BatchRecipe>(dto.BatchRecipeId);
        if (batchRecipeResult.Failure)
            return Response<LayerRecipe>.Fail(batchRecipeResult.Message, batchRecipeResult.Errors.ToArray());
        var batchRecipe = batchRecipeResult.Data!;
        
        var layerTypeResult = await _db.FindByStringAsync<LayerType>(dto.LayerTypeId);
        if (layerTypeResult.Failure)
            return Response<LayerRecipe>.Fail(layerTypeResult.Message, layerTypeResult.Errors.ToArray());
        var layerType = layerTypeResult.Data!;
        
        var maskResult = await _db.FindNullableByStringAsync<Mask>(dto.MaskId);
        if (maskResult.Failure)
            return Response<LayerRecipe>.Fail(maskResult.Message, maskResult.Errors.ToArray());
        var mask = maskResult.Data;
        
        List<LayerComponent> layerComponents = [];

        foreach (var layerComponentDto in dto.LayerComponents)
        {
            var result = await layerComponentService.CreateAsync(layerComponentDto);
            if (result.Failure || result.Data == null)
                return Response<LayerRecipe>.Fail(result.Message, result.Errors.ToArray());

            layerComponents.Add(result.Data);
        }

        var layerRecipe = new LayerRecipe(dto.LayerNumber, batchRecipe, layerType, layerComponents, mask, dto.Description);
        
        return await CreateAsync(layerRecipe);
    }

    public async Task<Response<LayerRecipe>> UpdateAsync(UpdateLayerRecipeDto dto)
    {
        var layerRecipeResult = await _db.FindByStringAsync<LayerRecipe>(dto.LayerRecipeId);
        if (layerRecipeResult.Failure)
            return layerRecipeResult;
        var layerRecipe = layerRecipeResult.Data!;

        if (dto.LayerNumber is not > 0)
            return "Номер слоя должен быть положительным";
        layerRecipe.LayerNumber = (int)dto.LayerNumber;
        
        if (!string.IsNullOrWhiteSpace(dto.LayerTypeId))
        {
            var layerTypeResult = await _db.FindByStringAsync<LayerType>(dto.LayerTypeId);
            if (layerTypeResult.Failure)
                return Response<LayerRecipe>.Fail(layerTypeResult.Message, layerTypeResult.Errors.ToArray());
            layerRecipe.LayerType = layerTypeResult.Data!;
        }

        if (!string.IsNullOrWhiteSpace(dto.MaskId))
        {
            var maskResult = await _db.FindByStringAsync<Mask>(dto.MaskId);
            if (maskResult.Failure)
                return Response<LayerRecipe>.Fail(maskResult.Message, maskResult.Errors.ToArray());
            layerRecipe.Mask = maskResult.Data;
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
            layerRecipe.Description = dto.Description;

        if (dto.LayerComponents is { Count: > 0 })
        {
            foreach (var compDto in dto.LayerComponents)
            {
                if (string.IsNullOrWhiteSpace(compDto.LayerComponentId))
                {
                    var createDto = new CreateLayerComponentDto
                    {
                        MaterialId = compDto.MaterialId!,
                        MaterialCodeId = compDto.MaterialCodeId!,
                        Thickness = compDto.Thickness!
                    };

                    var createResult = await layerComponentService.CreateAsync(createDto);
                    if (createResult.Failure)
                        return Response<LayerRecipe>.Fail(createResult.Message, createResult.Errors.ToArray());

                    layerRecipe.LayerComponents.Add(createResult.Data!);
                }
                else
                {
                    var updateDto = new UpdateLayerComponentDto
                    {
                        LayerComponentId = compDto.LayerComponentId,
                        MaterialId = compDto.MaterialId,
                        MaterialCodeId = compDto.MaterialCodeId,
                        Thickness = compDto.Thickness
                    };

                    var updateResult = await layerComponentService.UpdateAsync(updateDto);
                    if (updateResult.Failure)
                        return Response<LayerRecipe>.Fail(updateResult.Message, updateResult.Errors.ToArray());
                }
            }
        }

        await _db.SaveChangesAsync();
        return layerRecipe;
    }
    
    public async Task<Response<List<EntityDeletionInfo>>> RestoreAsync(string layerRecipeId)
    {
        var layerRecipeResult = await _db.FindByStringAsync<LayerRecipe>(layerRecipeId);
        if (layerRecipeResult.Failure)
            return Response<List<EntityDeletionInfo>>.Fail(layerRecipeResult.Message, layerRecipeResult.Errors.ToArray());
        var layerRecipe = layerRecipeResult.Data!;

        return await RestoreAsync(layerRecipe);
    } 

}