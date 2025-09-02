using System.Collections;
using Batch.Context;
using Batch.Models;
using Batch.Models.Displays;
using Batch.Models.DTO;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleService;
using Microsoft.EntityFrameworkCore;

namespace Batch.Services;

public class DisplayTypeService(BatchDbContext db) : SimpleService<DisplayType, BatchDbContext>(db)
{
    public async Task<Response<DisplayType>> CreateDisplayTypeAsync(DisplayTypeCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return "Введите имя типа дисплея.";

        if (dto.Name.Length > 100)
            return $"Имя типа дисплея не может быть больше 100 символов.";

        if (dto.Resolution == null || dto.Format == null || dto.ScreenSize == null)
            return "Введите разрешение экрана, формат и его размеры.";

        if (dto.AmountRows <= 0 || dto.AmountColumns <= 0)
            return "Количество строк и столбцов должно быть больше 0.";

        if (dto.CornersFormat != null && dto.CornersFormat.Any(x => x.Any(y => y <= 0)))
            return "Формат углов должен состоять из положительных значений.";

        var res = _isCornerFormatValid(dto.CornersFormat);
        if (res.Failure)
            return res.Message ?? string.Empty;

        if (await Db.DisplayTypes.AnyAsync(x => x.Name == dto.Name))
            return $"Тип дисплея с именем = '{dto.Name}' уже существует.";

        var resolution = new Size(dto.Resolution.Width!.Value, dto.Resolution.Height!.Value);
        var format = new Size(dto.Format.Width!.Value, dto.Format.Height!.Value);
        var screenSize = new Size(dto.ScreenSize.Width!.Value, dto.ScreenSize.Height!.Value);

        var entity = new DisplayType(
            dto.Name,
            resolution,
            format,
            screenSize,
            dto.AmountRows!.Value,
            dto.AmountColumns!.Value,
            dto.CornersFormat!)
        {
            Comment = dto.Comment
        };

        return await CreateAsync(entity);
    }

    public async Task<Response<DisplayType>> UpdateDisplayTypeAsync(DisplayTypeUpdateDto dto)
    {
        if (!Guid.TryParse(dto.Id, out var id))
            return "Id имеет неверный формат GUID.";

        var displayType = await Db.DisplayTypes.FindAsync(id);
        if (displayType == null)
            return $"Тип дисплея с id = {dto.Id} не найден.";

        if (dto.Name != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return "Имя не может быть пустым.";
            if (dto.Name.Length > 100)
                return $"Имя типа дисплея не может быть больше 100 символов.";

            var exists = await Db.DisplayTypes
                .AnyAsync(x => x.Name == dto.Name && x.Id != displayType.Id);
            if (exists)
                return $"Другой дисплей с именем '{dto.Name}' уже существует.";

            displayType.Name = dto.Name;
        }

        if (dto.Resolution != null)
            displayType.Resolution = new Size(dto.Resolution.Width!.Value, dto.Resolution.Height!.Value);

        if (dto.Format != null)
            displayType.Format = new Size(dto.Format.Width!.Value, dto.Format.Height!.Value);

        if (dto.ScreenSize != null)
            displayType.ScreenSize = new Size(dto.ScreenSize.Width!.Value, dto.ScreenSize.Height!.Value);

        if (dto.Comment != null)
            displayType.Comment = dto.Comment;

        return await UpdateAsync(displayType);
    }

    public async Task<Response<int>> SoftDeleteDisplayTypeAsync(string id)
    {
        if (!Guid.TryParse(id, out var displayTypeId))
            return "Id имеет неверный формат GUID.";
        
        var displayType = await Db.DisplayTypes.FindAsync(displayTypeId);
        if (displayType == null)
            return $"Тип дисплея с id = {id} не найден";
        
        return await SoftDeleteAsync(displayType);
    }

    public async Task<Response<int>> RestoreDisplayTypeAsync(string id)
    {
        if (!Guid.TryParse(id, out var displayTypeId))
            return "Id имеет неверный формат GUID.";
        
        var displayType = await Db.DisplayTypes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == displayTypeId);
        if (displayType == null)
            return $"Тип дисплея с id = {id} не найден";
        
        return await RestoreAsync(displayType);
    }


    
    private static Response<object> _isCornerFormatValid(object? value)
    {
        if (value is null) 
            return "";

        if (value is not IEnumerable outer)
            return "Неверный формат: ожидается массив массивов целых чисел.";

        foreach (var innerObj in outer)
        {
            if (innerObj is not IEnumerable inner)
                return "Неверный формат: каждый элемент должен быть массивом целых чисел.";

            using var e = inner.Cast<int>().GetEnumerator();
            if (!e.MoveNext())
                continue;

            var prev = e.Current;
            while (e.MoveNext())
            {
                var cur = e.Current;
                if (prev < cur)
                    return "Каждый подмассив должен быть по убыванию (a[i] >= a[i+1]).";
                prev = cur;
            }
        }

        return value;
    }
}