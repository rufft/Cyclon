using System.ComponentModel.DataAnnotations;
using Cyclone.Common.SimpleClient;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Measurement.Context;
using Measurement.Dto;
using Measurement.GraphQL;
using Measurement.Models.MeasureTypes;
using ILogger = Serilog.ILogger;

namespace Measurement.Services;

public class ViewMeasureService(
    MeasureDbContext db,
    FileService<MeasureDbContext> fileService,
    SimpleClient client,
    ILogger logger) : SimpleService<ViewMeasure, MeasureDbContext>(db, logger)
{
    private readonly MeasureDbContext _db = db;
    
    public async Task<Response<ViewMeasure>> CreateAsync(CreateViewMeasureDto dto)
    {
        if (!Guid.TryParse(dto.DisplayId, out var displayId))
            return "Id не в формате Guid";
        var displayResponse = await client.GetIdByIdAsync("Display", displayId);
        if (displayResponse.Failure)
            return "Не удалось получить дисплей по id";
        
        if (dto.ImageFile is null)
            return await CreateAsync(new ViewMeasure(displayId, dto.IsDefected));

        var screenSizeResponse = await client.ExecutePathAsync<SizeDto>(
            QueryTemplates.ScreenSizeQuery,
            "displayById.displayType.screenSize",
            new { id = displayResponse.Data }
        );
        if (screenSizeResponse.Failure)
            return Response<ViewMeasure>.Fail("Не удалось получить форматы дисплея " + screenSizeResponse.Message,
                screenSizeResponse.Errors.ToArray());
        var screenSize = screenSizeResponse.Data!;
        
        var imageResponse = await fileService.UploadImageAsync(dto.ImageFile);
        if (imageResponse.Failure || imageResponse.Data is null)
            return Response<ViewMeasure>.Fail("Не удалось загрузить фотографию" + imageResponse.Message,
                imageResponse.Errors.ToArray());
        var image = imageResponse.Data!;
        
        var compressedImageResponse = await fileService.CompressImageAndUploadAsync(
            image,
            screenSize.Width,
            screenSize.Height);
        if (compressedImageResponse.Failure || compressedImageResponse.Data is null)
            return Response<ViewMeasure>.Fail("Не удалось загрузить .ico" + imageResponse.Message,
                imageResponse.Errors.ToArray());
        var compresedImage = compressedImageResponse.Data!;

        return await CreateAsync(new ViewMeasure(displayId, dto.IsDefected, image, compresedImage));
    }
    
    public async Task<Response<List<EntityDeletionInfo>>> DeleteAsync([Required] string? id)
    {
        if (id == null)
            return "Введите Id";
        if (!Guid.TryParse(id, out var viewMeasureId))
            return "Id не в формате Guid";
         
        var viewMeasure = await _db.FindAsync<ViewMeasure>(viewMeasureId);

        if (viewMeasure is null)
            return $"View измерения с id-- {viewMeasureId} не существует";
         
        return await SoftDeleteAsync(viewMeasure);
    }
}

public record SizeDto(double Height, double Width);