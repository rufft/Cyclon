using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Cyclone.Common.SimpleEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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