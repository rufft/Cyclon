using System.ComponentModel.DataAnnotations;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleSoftDelete;
using HotChocolate;
using Recipes.Dto;
using Recipes.Models;
using Recipes.Services;

namespace Recipes.GraphQL;

public class Mutation
{
    // BatchRecipe
    public async Task<Response<BatchRecipe>> CreateBatchRecipeAsync(
        [Service] BatchRecipeService batchRecipeService,
        CreateBatchRecipeDto input) => await batchRecipeService.CreateAsync(input);

    public async Task<Response<BatchRecipe>> UpdateBatchRecipeAsync(
        [Service] BatchRecipeService batchRecipeService,
        UpdateBatchRecipeDto input) => await batchRecipeService.UpdateAsync(input);
    
    public async Task<Response<List<EntityDeletionInfo>>> DeleteLayerRecipeAsync(
        [Service] BatchRecipeService batchRecipeService,
        [Required] string? id) => await batchRecipeService.DeleteLayerAsync(id);

    public async Task<Response<List<EntityDeletionInfo>>> DeleteBatchRecipeAsync(
        [Service] BatchRecipeService batchRecipeService,
        [Required] string? id) => await batchRecipeService.DeleteAsync(id);
    

    // Material
    public async Task<Response<Material>> CreateMaterialAsync(
        [Service] MaterialService materialService,
        string input) => await materialService.CreateAsync(input);

    public async Task<Response<List<EntityDeletionInfo>>> DeleteMaterialAsync(
        [Service] MaterialService materialService,
        [Required] string? id) => await materialService.DeleteAsync(id);

    // MaterialCode
    public async Task<Response<MaterialCode>> CreateMaterialCodeAsync(
        [Service] MaterialCodeService materialCodeService,
        string input) => await materialCodeService.CreateAsync(input);
    
    public async Task<Response<List<EntityDeletionInfo>>> DeleteMaterialCodeAsync(
        [Service] MaterialCodeService materialCodeService,
        [Required] string? id) => await materialCodeService.DeleteAsync(id);

    // LayerType
    public async Task<Response<LayerType>> CreateLayerTypeAsync(
        [Service] LayerTypeService layerTypeService,
        string input) => await layerTypeService.CreateAsync(input);

    public async Task<Response<List<EntityDeletionInfo>>> DeleteLayerTypeAsync(
        [Service] LayerTypeService layerTypeService,
        [Required] string? id) => await layerTypeService.DeleteAsync(id);

    // Mask
    public async Task<Response<Mask>> CreateMaskAsync(
        [Service] MaskService maskService,
        string input) => await maskService.CreateAsync(input);

    public async Task<Response<List<EntityDeletionInfo>>> DeleteMaskAsync(
        [Service] MaskService maskService,
        [Required] string? id) => await maskService.DeleteAsync(id);
}