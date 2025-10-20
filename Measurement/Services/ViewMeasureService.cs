using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Batch.Models.Displays;
using Cyclone.Common.SimpleClient;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using HotChocolate.Types.Composite;
using Measurement.Context;
using Measurement.Dto;
using Measurement.Models.MeasureTypes;

namespace Measurement.Services;

public class ViewMeasureService(
    MeasureDbContext db,
    FileService<MeasureDbContext> fileService,
    SimpleClient client,
    ILogger<ViewMeasureService> logger) : SimpleService<ViewMeasure, MeasureDbContext>(db)
{
    private const string ScreensizeQuery = """
                                           query ($id: UUID!) {
                                             displayById(id: $id) {
                                               displayType { screenSize { height width } }
                                             }
                                           }
                                           """;
    
    public async Task<Response<ViewMeasure>> CreateAsync(CreateViewMeasureDto dto)
    {
        if (!Guid.TryParse(dto.DisplayId, out var displayId))
            return "Id не в формате Guid";
        var displayResponse = await client.GetIdByIdAsync<Display>(displayId);
        if (displayResponse.Failure)
            return "Не удалось получить дисплей по id";
        
        if (dto.ImageFile is null)
            return await CreateAsync(new ViewMeasure(displayId, dto.IsDefected));

        var screensizeResponse = await client.ExecutePathAsync<SizeDto>(
            ScreensizeQuery,
            "displayById.displayType.screenSize",
            new { id = displayResponse.Data }
        );
        if (screensizeResponse.Failure)
            return Response<ViewMeasure>.Fail("Не удалось получить форматы дисплея " + screensizeResponse.Message,
                screensizeResponse.Errors.ToArray());
        var screensize = screensizeResponse.Data!;
        
        var imageResponse = await fileService.UploadImageAsync(dto.ImageFile);
        if (imageResponse.Failure || imageResponse.Data is null)
            return Response<ViewMeasure>.Fail("Не удалось загрузить фотографию" + imageResponse.Message,
                imageResponse.Errors.ToArray());
        var image = imageResponse.Data!;
        
        var compresedImageResponse = await fileService.CompressImageAndUploadAsync(
            image,
            screensize.Width,
            screensize.Height);
        if (compresedImageResponse.Failure || compresedImageResponse.Data is null)
            return Response<ViewMeasure>.Fail("Не удалось загрузить .ico" + imageResponse.Message,
                imageResponse.Errors.ToArray());
        var compresedImage = compresedImageResponse.Data!;

        return await CreateAsync(new ViewMeasure(displayId, dto.IsDefected, image, compresedImage));
    }
    
    public async Task<Response<List<DeleteEntityInfo>>> DeleteAsync([Required] string? id)
    {
        if (id == null)
            return "Введите Id";
        if (!Guid.TryParse(id, out var viewMeasureId))
            return "Id не в формате Guid";
         
        var viewMeasure = await db.FindAsync<ViewMeasure>(viewMeasureId);

        if (viewMeasure is null)
            return $"View измерения с id-- {viewMeasureId} не существует";
         
        return await SoftDeleteAsync(viewMeasure);
    }
}

public record SizeDto(double Height, double Width);