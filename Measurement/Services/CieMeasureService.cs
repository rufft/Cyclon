using Batch.Models.Displays;
using Cyclone.Common.SimpleClient;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using HotChocolate.Types.Composite;
using Measurement.Context;
using Measurement.Dto;
using Measurement.Models.MeasureTypes;
using ILogger = Serilog.ILogger;

namespace Measurement.Services;

public class CieMeasureService(MeasureDbContext db, SimpleClient client, ILogger logger) : SimpleService<CieMeasure, MeasureDbContext>(db, logger)
{
    private readonly MeasureDbContext _db = db;

    public async Task<Response<CieMeasure>> CreateAsync(CreateCieMeasureDto dto)
     {
         if (dto.CieX is null or < 0 and > 1)
             return "Cie x должен быть от 0 до 1";
         var cieX = (double)dto.CieX;
         if (dto.CieY is null or < 0 or > 1)
             return "Cie y должен быть от 0 до 1";
         var cieY = (double)dto.CieY;
         if (dto.Lv is null or < 0)
             return "Lv не может быть отрицательным";
         var lv = (double)dto.Lv;
         if (dto.DisplayId == null)
             return "Введите DisplayId";
         if (!Guid.TryParse(dto.DisplayId, out var expectedDisplayId))
             return "Id не в формате Guid";
         
         var response = await client.GetIdByIdAsync<Display>(expectedDisplayId);
         
         if (response.Failure)
             return Response<CieMeasure>.Fail(message: response.Message, errors: response.Errors.ToArray());
         
         if (response.Data is null)
             return Response<CieMeasure>.Fail(
                 message: $"Дисплея с id-- {expectedDisplayId} не существует",
                 errors: response.Errors.ToArray());
         
         var displayId = response.Data.Value;
         
         var cieMeasure = new CieMeasure(displayId, cieX, cieY, lv);
    
         return await CreateAsync(cieMeasure);
    }

     public async Task<Response<CieMeasure>> UpdateAsync(UpdateCieMeasureDto dto)
     {
         if (dto.CieX == null && dto.CieY == null && dto.Lv == null)
             return "Введите значения для изменения";
         if (dto.CieX is < 0 or > 1)
             return "Cie должен быть от 0 до 1";
         if (dto.CieY is < 0 or > 1)
             return "Cie должен быть от 0 до 1";
         if (dto.Lv is < 0)
             return "Lv должен быт положительным";
         if (dto.Id == null)
             return "Введите Id";
         if (!Guid.TryParse(dto.Id, out var cieId))
             return "Id не в формате Guid";
         
         var cieMeasure = await _db.FindAsync<CieMeasure>(cieId);

         if (cieMeasure is null)
             return $"Cie измерения с id-- {cieId} не существует";
         
         
         if (dto.CieX is not null)
             cieMeasure.Cie.X = (double)dto.CieX;
         
         if (dto.CieY is not null)
             cieMeasure.Cie.Y = (double)dto.CieY;
         
         if (dto.Lv is not null)
             cieMeasure.Lv = (double)dto.Lv;
         
         return await UpdateAsync(cieMeasure);
     }

     public async Task<Response<List<DeleteEntityInfo>>> DeleteAsync([Require] string? id)
     {
         if (id == null)
             return "Введите Id";
         if (!Guid.TryParse(id, out var cieId))
             return "Id не в формате Guid";
         
         var cieMeasure = await _db.FindAsync<CieMeasure>(cieId);

         if (cieMeasure is null)
             return $"Cie измерения с id-- {cieId} не существует";
         
         return await SoftDeleteAsync(cieMeasure);
     }
}