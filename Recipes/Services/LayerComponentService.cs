using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Recipes.Context;
using Recipes.Dto;
using Recipes.Models;
using ILogger = Serilog.ILogger;

namespace Recipes.Services;

public class LayerComponentService(RecipeDbContext db, ILogger logger) : SimpleService<LayerComponent, RecipeDbContext>(db, logger)
{
    public async Task<Response<LayerComponent>> CreateAsync(CreateLayerComponentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LayerRecipeId))
            return "Введите id слоя";
        
        if (string.IsNullOrWhiteSpace(dto.Thickness))
            return "Введите толщину слоя";

        if (string.IsNullOrWhiteSpace(dto.MaterialId))
            return "Введите id материала";

        if (string.IsNullOrWhiteSpace(dto.MaterialCodeId))
            return "Введите id кода материала";

        if (!Guid.TryParse(dto.LayerRecipeId, out var layerRecipeId))
            return "Id слоя не в формате Guid";
        if (!Guid.TryParse(dto.MaterialId, out var materialId))
            return "Id материала не в формате Guid";
        if (!Guid.TryParse(dto.MaterialCodeId, out var materialCodeId))
            return "Id кода материала не в формате Guid";
        
        var layerRecipe = await db.FindAsync<LayerRecipe>(layerRecipeId);
        if (layerRecipe == null)
            return $"Слоя с id-- {layerRecipeId} не существует";
        
        var material = await db.FindAsync<Material>(materialId);
        if (material == null)
            return $"Материал с id-- {materialId} не существует";
        
        var materialCode = await db.FindAsync<MaterialCode>(materialCodeId);
        if (materialCode == null)
            return $"Кода материала с id-- {materialCodeId} не существует";
        
        var layerComponent = new LayerComponent(layerRecipe, materialCode, material, dto.Thickness);
        
        return await CreateAsync(layerComponent);
    }

    public async Task<Response<LayerComponent>> UpdateAsync(UpdateLayerComponentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LayerComponentId))
            return "Введите id компонента слоя";
        if (!Guid.TryParse(dto.LayerComponentId, out var layerComponentId))
            return "Id компанента слоя не в формате Guid";
        
        var layerComponent = await db.FindAsync<LayerComponent>(layerComponentId);

        if (layerComponent == null)
            return $"Компанента слоя с id-- {layerComponentId} не существует"; 
        
        if (!string.IsNullOrWhiteSpace(dto.MaterialCodeId))
        {
            if (!Guid.TryParse(dto.MaterialCodeId, out var materialCodeId))
                return "Id кода материала не в формате Guid";
        
            var materialCode = await db.FindAsync<MaterialCode>(materialCodeId);
            
            if (materialCode == null)
                return $"Кода материала с id-- {materialCodeId} не существует";
            
            layerComponent.MaterialCode = materialCode;
        }
        
        if (!string.IsNullOrWhiteSpace(dto.MaterialId))
        {
            if (!Guid.TryParse(dto.MaterialId, out var materialId))
                return "Id материала не в формате Guid";
        
            var material = await db.FindAsync<Material>(materialId);
            
            if (material == null)
                return $"Материала с id-- {materialId} не существует";
            
            layerComponent.Material = material;
        }
        
        if (!string.IsNullOrWhiteSpace(dto.Thickness))
            layerComponent.Thickness = dto.Thickness;
        
        return await UpdateAsync(layerComponent);
    }
}