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

public class PowerMeasureService(
    MeasureDbContext db,
    SimpleClient client, ILogger logger) : SimpleService<PowerMeasure, MeasureDbContext>(db, logger)
{
    private readonly MeasureDbContext _db = db;

    public async Task<Response<PowerMeasure>> CreateAsync(CreatePowerMeasureDto dto)
    {
        if (dto.PowerPairs is null)
            return "Введите значения тока и напряжения";
        if (dto.PowerPairs.Length is not (1 or 3))
            return $"Неверно количество каналов ({dto.PowerPairs.Length})";

        if (dto.DisplayId == null)
            return "Введите DisplayId";
        if (!Guid.TryParse(dto.DisplayId, out var expectedDisplayId))
            return $"Id не в формате Guid ({dto.DisplayId})";

        var response = await client.GetIdByIdAsync("Display", expectedDisplayId);

        if (response.Failure)
            return Response<PowerMeasure>.Fail(message: response.Message, errors: response.Errors.ToArray());

        if (response.Data is null)
            return Response<PowerMeasure>.Fail(
                message: $"Дисплея с id-- {expectedDisplayId} не существует",
                errors: response.Errors.ToArray());

        var displayId = response.Data.Value;
        
        var batchIdResponse = await client.ExecutePathAsync<Guid?>(
            QueryTemplates.BatchIdByDisplayId,
            "displayById.batchId",
            new { id = displayId }
        );
         
        if (batchIdResponse.Failure)
            return Response<PowerMeasure>.Fail(message: batchIdResponse.Message, errors: batchIdResponse.Errors);
         
        if (batchIdResponse.Data == null)
            return Response<PowerMeasure>.Fail(
                message: $"Дисплея с id-- {expectedDisplayId} не существует",
                errors: batchIdResponse.Errors);
        var batchId = batchIdResponse.Data.Value;

        var powerMeasure = new PowerMeasure(batchId, displayId, dto.PowerPairs.ToList(), dto.ReversePowerPairs);

        return await CreateAsync(powerMeasure);
    }

    public async Task<Response<PowerMeasure>> UpdateAsync(UpdatePowerMeasureDto dto)
    {
        if (dto.PowerPairs is null && dto.ReversePowerPairs is null)
            return "Введите значения";
        if (dto.Id == null)
            return "Введите Id";
        if (!Guid.TryParse(dto.Id, out var powerId))
            return "Id не в формате Guid";

        var powerMeasure = await _db.FindAsync<PowerMeasure>(powerId);

        if (powerMeasure is null)
            return $"Power измерения с id-- {powerId} не существует";

        if (dto.PowerPairs != null && PowerPair.IsInCorrectChanel(dto.PowerPairs.ToList()))
            powerMeasure.PowerPairs = dto.PowerPairs.ToList();

        if (dto.ReversePowerPairs != null)
            powerMeasure.ReversePowerPair = dto.ReversePowerPairs;

        return await UpdateAsync(powerMeasure);
    }

    public async Task<Response<List<EntityDeletionInfo>>> DeleteAsync([Required] string? id)
    {
        if (id == null)
            return "Введите Id";
        if (!Guid.TryParse(id, out var powerMeasureId))
            return "Id не в формате Guid";
         
        var powerMeasure = await _db.FindAsync<PowerMeasure>(powerMeasureId);

        if (powerMeasure is null)
            return $"Power измерения с id-- {powerMeasureId} не существует";
         
        return await SoftDeleteAsync(powerMeasure);
    }

}