using Cyclone.Common.SimpleService;
using Measurement.Context;
using Measurement.Dto;
using Measurement.Models.MeasureTypes;

namespace Measurement.Services;

public class CieMeasureService(MeasureDbContext db) : SimpleService<CieMeasure, MeasureDbContext>(db)
{
    // public async Task<Response<CieMeasure>> CreateAsync(CreateCieMeasureDto dto)
    // {
    //     if (dto.CieX is null or < 0 and > 1)
    //         return "Cie x должен быть от 0 до 1";
    //     var cieX = (double)dto.CieX;
    //     if (dto.CieY is null or < 0 or > 1)
    //         return "Cie y должен быть от 0 до 1";
    //     var cieY = (double)dto.CieY;
    //     if (dto.Lv is null or < 0)
    //         return "Lv не может быть отрицательным";
    //     var lv = (double)dto.Lv;
    //     if (dto.DisplayId == null)
    //         return "Введите DisplayId";
    //     var expectedDisplayId = (Guid)dto.DisplayId;
    //     
    //     var operationResult = await batchClient.TryGetDisplayById.ExecuteAsync(expectedDisplayId);
    //     
    //     if (operationResult.Data?.DisplayById == null)
    //         return "Дисплея с таким id нет";
    //     
    //     var displayId = operationResult.Data.DisplayById.Id;
    //     
    //     var cieMeasure = new CieMeasure(displayId, cieX, cieY, lv);
    //
    //     return await CreateAsync(cieMeasure);
    //}
}