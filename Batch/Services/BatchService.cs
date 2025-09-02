using Batch.Context;
using Batch.Extensions;
using Batch.Models;
using Batch.Models.Displays;
using Batch.Models.DTO;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Microsoft.EntityFrameworkCore;
using static Batch.Extensions.Validation.ValidationConstants;


namespace Batch.Services;

public class BatchService(BatchDbContext db) : SimpleService<Models.Batch, BatchDbContext>(db)
{
    public async Task<Response<Models.Batch>> CreateBatchAsync(BatchCreateDto dto)
    {
        if (dto.Number is <= BATCH_NUM_MIN or > BATCH_NUM_MAX)
            return$"Номер партии не может быть меньше {BATCH_NUM_MIN} или быть больше {BATCH_NUM_MAX}";
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > BATCH_NAME_MAX)
            return $"Имя не может быть пустым или быть больше {BATCH_NAME_MAX} символов";
        if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > BATCH_DESC_MAX)
            return $"Описание не может быть больше {BATCH_DESC_MAX}";
        if (!Guid.TryParse(dto.DisplayTypeId, out var displayTypeId))
            return "DisplayTypeId имеет неверный формат GUID.";
        if (!Enum.TryParse<DisplayColor>(dto.Color, ignoreCase: true, out var color))
            return $"Неверный формат цвета: {dto.Color}";
        
        
        var displayType = await Db.FindAsync<DisplayType>(displayTypeId);
        if (displayType == null)
            return $"Тип дисплея с id-- {displayTypeId} не существует";
        
        var coverResponse = CoverHelper.FromCoverArray(dto.Cover, displayType.AmountRows);
        
        if (coverResponse.Failure)
            return Response<Models.Batch>.Fail(coverResponse.Message);

        Models.Batch batch = new(dto.Number!.Value, dto.Name, displayType, color, coverResponse.Data, dto.Description);

        return await CreateAsync(batch);
    }

    public async Task<Response<Models.Batch>> UpdateBatchAsync(BatchUpdateDto dto)
    {
        if (!Guid.TryParse(dto.Id, out var id))
            return "Id имеет неверный формат GUID.";
        var batch = await Db.FindAsync<Models.Batch>(id);
        if (batch == null)
            return $"Партии с id-- {dto.Id} не существует";
        
        if (dto.Number is <= BATCH_NUM_MIN or > BATCH_NUM_MAX)
            return$"Номер партии не может быть меньше {BATCH_NUM_MIN} или быть больше {BATCH_NUM_MAX}";
        batch.Number = dto.Number!.Value;
            
        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Length > BATCH_NAME_MAX)
            return $"Имя не может быть больше {BATCH_NAME_MAX} символов";
        batch.Name = dto.Name!;
        
        if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > BATCH_DESC_MAX)
            return $"Описание не может быть больше {BATCH_DESC_MAX}";
        batch.Description = dto.Description;
        
        if (!string.IsNullOrWhiteSpace(dto.Color) 
            && !Enum.TryParse<DisplayColor>(dto.Color, ignoreCase: true, out _))
            return $"Неверный формат цвета: {dto.Color}";
        if (Enum.TryParse<DisplayColor>(dto.Color, ignoreCase: true, out var color)) 
            batch.DisplayColor = color;
        
        return await UpdateAsync(batch);
    }

    public async Task<Response<int>> SoftDeleteBatchAsync(string id)
    {
        if (!Guid.TryParse(id, out var batchId))
            return "Id имеет неверный формат GUID.";
        
        var batch = await Db.Batches.FindAsync(batchId);
        if (batch == null)
            return $"Партия с id = {id} не найдена";
        
        return await SoftDeleteAsync(batch);
    }

    public async Task<Response<int>> RestoreBatchAsync(string id)
    {
        if (!Guid.TryParse(id, out var batchId))
            return "Id имеет неверный формат GUID.";
        
        var batch = await Db.Batches.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == batchId);
        if (batch == null)
            return $"Партия с id = {id} не найдена";
        
        return await RestoreAsync(batch);
    }
}