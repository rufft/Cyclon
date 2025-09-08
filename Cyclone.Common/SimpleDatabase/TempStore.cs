using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace Cyclone.Common.SimpleDatabase;

internal static class TempStore
{
    private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, object>> CtxData = new();

    private static Guid Key(DbContext ctx) => ctx.ContextId.InstanceId;

    public static void Set(DbContext ctx, string key, object value)
    {
        var bag = CtxData.GetOrAdd(Key(ctx), _ => new ConcurrentDictionary<string, object>());
        bag[key] = value;
    }

    public static void AppendList<T>(DbContext ctx, string key, IEnumerable<T> items)
    {
        var bag = CtxData.GetOrAdd(Key(ctx), _ => new ConcurrentDictionary<string, object>());
        if (bag.TryGetValue(key, out var obj) && obj is List<T> list)
            list.AddRange(items);
        else
            bag[key] = new List<T>(items);
    }

    public static bool TryGet<T>(DbContext ctx, string key, out T? value)
    {
        value = default;
        if (!CtxData.TryGetValue(Key(ctx), out var bag) ||
            !bag.TryGetValue(key, out var obj) ||
            obj is not T t) return false;
        value = t;
        return true;
    }

    public static void Remove(DbContext ctx, string key)
    {
        if (CtxData.TryGetValue(Key(ctx), out var bag))
            bag.TryRemove(key, out _);
    }

    public static void Clear(DbContext ctx) => CtxData.TryRemove(Key(ctx), out _);
}