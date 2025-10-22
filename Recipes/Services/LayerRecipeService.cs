using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Recipes.Context;
using Recipes.Dto;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class LayerRecipeService(RecipeDbContext db, ILogger logger, LayerComponentService layerComponentService)
    : SimpleService<LayerRecipe, RecipeDbContext>(db, logger)
{
    public async Task<Response<LayerRecipe>> CreateAsync(CreateLayerRecipeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BatchRecipeId))
            return "Введите id рецепта партии слоя";
        if (string.IsNullOrWhiteSpace(dto.LayerTypeId))
            return "Введите id типа слоя";

        Mask? mask = null;
        if (!string.IsNullOrWhiteSpace(dto.MaskId))
        {
            if (!Guid.TryParse(dto.MaskId, out var maskId))
                return "Id маски не в формате Guid";
            mask = await db.FindAsync<Mask>(maskId);
            if (mask == null)
                return $"Маски с id-- {maskId} не существует";
        }
        
        if (!Guid.TryParse(dto.BatchRecipeId, out var batchRecipeId))
            return "Id типа слоя не в формате Guid";
        if (!Guid.TryParse(dto.LayerTypeId, out var layerTypeId))
            return "Id типа слоя не в формате Guid";

        var layerType = await db.FindAsync<LayerType>(layerTypeId);
        var batchRecipe = await db.FindAsync<BatchRecipe>(batchRecipeId);
        
        if (layerType == null)
            return $"Типа слоя с id-- {layerTypeId} не существует";
        if (batchRecipe == null)
            return $"Рецепта партии с id-- {batchRecipeId} не существует";
        
        List<LayerComponent> layerComponents = [];

        foreach (var layerComponentDto in dto.LayerComponents)
        {
            var result = await layerComponentService.CreateAsync(layerComponentDto);
            if (result.Failure || result.Data == null)
                return Response<LayerRecipe>.Fail(result.Message, result.Errors.ToArray());

            layerComponents.Add(result.Data);
        }

        var layerRecipe = new LayerRecipe(batchRecipe, layerType, layerComponents, mask, dto.Description);
        
        return await CreateAsync(layerRecipe);
    }
    
    
}