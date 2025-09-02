using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Cyclone.Common.SimpleEntity;
using Microsoft.EntityFrameworkCore;

namespace Cyclone.Common.SimpleDatabase;

/// <summary>
/// Общий базовый DbContext.
/// Передавайте в конструктор сборки, где лежат entity (domain assembly),
/// либо унаследуйте и передайте assembly в конструктор наследника.
/// </summary>
public class SimpleDbContext(DbContextOptions options,
    params Assembly[]? entityAssemblies)
    : DbContext(options)
{
    private readonly Assembly[] _entityAssemblies = entityAssemblies ?? [];
    public override object? Find(Type entityType, params object?[]? keyValues)
        => base.Find(entityType, CoerceKeys(entityType, keyValues));

    public override ValueTask<object?> FindAsync(Type entityType, params object?[]? keyValues)
        => base.FindAsync(entityType, CoerceKeys(entityType, keyValues));

    public override ValueTask<object?> FindAsync(Type entityType, object?[]? keyValues, CancellationToken cancellationToken)
        => base.FindAsync(entityType, CoerceKeys(entityType, keyValues), cancellationToken);

    private static object?[]? CoerceKeys(Type entityType, object?[]? keyValues)
    {
        if (keyValues is null || keyValues.Length != 1)
            return keyValues;

        var k = keyValues[0];

        switch (k)
        {
            case Guid:
                break;
            case string s when Guid.TryParse(s, out var g):
                return [g];
            case string relay when LooksLikeBase64(relay) && IsGuidPkEntity(entityType):
                try
                {
                    var raw = Encoding.UTF8.GetString(Convert.FromBase64String(relay));

                    var parts = raw.Split(':');
                    string? typeName = null;
                    string? idPart = null;

                    switch (parts.Length)
                    {
                        case 2:
                            typeName = parts[0];
                            idPart = parts[1];
                            break;
                        case >= 3:
                            typeName = parts[^2];
                            idPart = parts[^1];
                            break;
                    }

                    if (!string.IsNullOrEmpty(typeName) &&
                        !string.Equals(typeName, entityType.Name, StringComparison.Ordinal))
                    {
                        return keyValues;
                    }

                    if (idPart is not null && Guid.TryParse(idPart, out var guidFromRelay))
                        return [guidFromRelay];
                }
                catch
                {
                    // ignored
                }

                break;
        }

        return keyValues;
    }

    private static bool IsGuidPkEntity(Type entityType) =>
        entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)?
            .PropertyType == typeof(Guid);

    private static bool LooksLikeBase64(string s) =>
        s.Length % 4 == 0 && s.Select(ch => ch is >= 'A' and <= 'Z' 
            or >= 'a' and <= 'z' 
            or >= '0' and <= '9' 
            or '+' or '/' or '=').All(ok => ok);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (_entityAssemblies.Length > 0)
        {
            foreach (var asm in _entityAssemblies)
            {
                var allTypes = GetTypesSafe(asm);
                var entityTypes = allTypes.Where(t =>
                        t is { IsClass: true, IsAbstract: false } &&
                        typeof(BaseEntity).IsAssignableFrom(t) &&
                        !Attribute.IsDefined(t, typeof(OwnedAttribute))
                );

                foreach (var t in entityTypes)
                {
                    modelBuilder.Entity(t);
                }
                try { modelBuilder.ApplyConfigurationsFromAssembly(asm); } catch { /* ignore */ }
            }

            RegisterEntityTypes(modelBuilder);
        }
        


        ApplyUtcDateTimeConverter(modelBuilder);
        ApplySoftDeleteFilters(modelBuilder);

        ConfigureDomainModel(modelBuilder);
    }

    /// <summary>
    /// Переопределяй в сервисе для добавления отношений, owned types, JSON conversions и т.д.
    /// </summary>
    protected virtual void ConfigureDomainModel(ModelBuilder modelBuilder) { }

    private void RegisterEntityTypes(ModelBuilder modelBuilder)
    {
        foreach (var asm in _entityAssemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

            foreach (var t in types)
            {
                if (!t.IsClass || t.IsAbstract) continue;
                if (!typeof(BaseEntity).IsAssignableFrom(t)) continue;
                if (Attribute.IsDefined(t, typeof(OwnedAttribute))) continue;

                modelBuilder.Entity(t);
            }
        }
    }

    private void ApplyUtcDateTimeConverter(ModelBuilder modelBuilder)
    {
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var et in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var prop in et.GetProperties())
            {
                if (prop.ClrType == typeof(DateTime))
                    prop.SetValueConverter(dateTimeConverter);
                else if (prop.ClrType == typeof(DateTime?))
                    prop.SetValueConverter(nullableConverter);
            }
        }
    }

    private void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var et in modelBuilder.Model.GetEntityTypes())
        {
            var clr = et.ClrType;

            var isDeletedProp = clr.GetProperty("IsDeleted");
            if (isDeletedProp == null || isDeletedProp.PropertyType != typeof(bool)) continue;

            var parameter = Expression.Parameter(clr, "e");

            var efProperty = typeof(EF)
                .GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(typeof(bool));

            var propertyAccess = Expression.Call(efProperty, parameter, Expression.Constant("IsDeleted"));
            var body = Expression.Equal(propertyAccess, Expression.Constant(false));
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clr).HasQueryFilter(lambda);
        }
    }
    
    private static IEnumerable<Type> GetTypesSafe(Assembly asm)
    {
        try
        {
            return asm.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).Cast<Type>();
        }
    }
}