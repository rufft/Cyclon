using Batch.Context;
using Microsoft.EntityFrameworkCore;

namespace Batch.Extensions;

[ExtendObjectType("Batch")]
public class BatchResolvers
{
    // В GraphQL имя поля будет "cover"
    [GraphQLName("cover")]
    public IEnumerable<int> GetCover(
        [Parent] Models.Batch batch,
        [Service] BatchDbContext db)  // получаем DbContext из DI
    {
        // Попробуем получить число рядов для типа дисплея (если у вас есть такая связка)
        var maxRows = 20; // дефолт
        var dt = db.DisplayTypes
            .AsNoTracking()
            .FirstOrDefault(d => d.Id == batch.DisplayTypeId);
        if (dt is { AmountRows: > 0 })
            maxRows = dt.AmountRows;

        var mask = batch.Cover; // предполагается, что CoverMask хранится как int
        foreach (var n in CoverHelper.ToNumbers(mask, maxRows))
            yield return n;
    }
}