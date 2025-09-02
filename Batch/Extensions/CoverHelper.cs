using Batch.Models;
using Cyclone.Common.SimpleResponse;

namespace Batch.Extensions;

public static class CoverHelper
{
    /// <summary>
    /// Преобразует входной массив в Cover flags по правилам:
    /// |null => All
    /// |[0] => None
    /// |[1,3,5] => соответствующие биты
    /// В: maxRows (по умолчанию 20)
    /// </summary>
    public static Response<Cover> FromCoverArray(int[]? coverRows, int maxRows = 20)
    {
        switch (coverRows)
        {
            case null:
                return Cover.All;
            case [0]:
                return Cover.None;
        }

        if (coverRows.Length == 0)
            return "Пустой массив cover недопустим. Не передавать поле => all; передать [0] => none; передать [1,2] => конкретные ряды.";

        if (coverRows.Any(x => x == 0))
            return "Если хотите None — используйте [0]. Нельзя совмещать 0 с номерами рядов.";

        var mask = Cover.None;
        var seen = new HashSet<int>();
        foreach (var r in coverRows)
        {
            if (r < 1 || r > maxRows)
                return $"Номер ряда {r} вне диапазона 1..{maxRows}.";

            if (!seen.Add(r)) continue;

            mask |= (Cover)(1 << (r - 1));
        }

        return mask;
    }

    public static IEnumerable<int> ToNumbers(Cover mask, int maxRows = 20)
    {
        for (var i = 1; i <= maxRows; i++)
        {
            if ((mask & (Cover)(1 << (i - 1))) != 0)
                yield return i;
        }
    }
}