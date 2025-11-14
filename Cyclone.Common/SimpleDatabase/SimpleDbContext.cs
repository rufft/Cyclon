using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Cyclone.Common.SimpleDatabase.FileSystem;
using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleResponse;
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
    private readonly bool _filesEnabled = options.FindExtension<FilesFeatureOptionsExtension>()?.Enabled ?? false;
    
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
                    !Attribute.IsDefined(t, typeof(OwnedAttribute)));

                if (!_filesEnabled)
                    entityTypes = entityTypes.Where(t => !typeof(IFileEntity).IsAssignableFrom(t));

                foreach (var t in entityTypes)
                    modelBuilder.Entity(t);

                try { modelBuilder.ApplyConfigurationsFromAssembly(asm); } catch { /* ignore */ }
            }

            RegisterEntityTypes(modelBuilder);
        }

        ApplyUtcDateTimeConverter(modelBuilder);
        ApplySoftDeleteFilters(modelBuilder);

        ConfigureDomainModel(modelBuilder);
    }

    protected virtual void ConfigureDomainModel(ModelBuilder modelBuilder) { }

    public async Task<Response<TEntity>> FindByStringAsync<TEntity>(string? id) where TEntity : BaseEntity
    {
        var response = TryParseStringToGuidResponse(id);
        
        if (response.Failure)
            return Response<TEntity>.Fail(response.Message, response.Errors.ToArray());
        
        var guidId = response.Data;
        
        var entity = await FindAsync<TEntity>(guidId);
        if (entity == null)
            return $"{typeof(TEntity)} с id-- {guidId} не существует";
        
        return entity;
    }

    public static Response<Guid> TryParseStringToGuidResponse(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Response<Guid>.Fail("Введите id");
        return !Guid.TryParse(id, out var guidId) 
            ? Response<Guid>.Fail("Id не в формате Guid") 
            : Response<Guid>.Ok(guidId);
    } 
    
    public async Task<Response<TEntity?>> FindNullableByStringAsync<TEntity>(string? id) where TEntity : BaseEntity
    {
        if (id == null)
            return Response<TEntity?>.Ok(null);
        
        if (string.IsNullOrWhiteSpace(id))
            return "Введите id";
        
        if (!Guid.TryParse(id, out var guidId))
            return "Id не в формате Guid";
        
        var entity = await FindAsync<TEntity>(guidId);
        if (entity == null)
            return $"{typeof(TEntity)} с id-- {guidId} не существует";
        
        return entity;
    }

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
                if (!_filesEnabled && typeof(IFileEntity).IsAssignableFrom(t)) continue;

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

            var isDeletedProp = clr.GetProperty(nameof(BaseEntity.IsDeleted));
            if (isDeletedProp == null || isDeletedProp.PropertyType != typeof(bool)) continue;

            var parameter = Expression.Parameter(clr, "e");

            var efProperty = typeof(EF)
                .GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(typeof(bool));

            var propertyAccess = Expression.Call(efProperty, parameter, Expression.Constant(nameof(BaseEntity.IsDeleted)));
            var body = Expression.Equal(propertyAccess, Expression.Constant(false));
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clr).HasQueryFilter(lambda);
        }
    }
    private static DateTime ToUtcKind(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt.ToUniversalTime(), DateTimeKind.Utc);

    
    private void EnsureDateTimesAreUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
            
            foreach (var prop in entry.Properties)
            {
                if (prop.Metadata.ClrType == typeof(DateTime))
                {
                    if (prop.CurrentValue is DateTime dt)
                        prop.CurrentValue = ToUtcKind(dt);
                }
                else if (prop.Metadata.ClrType == typeof(DateTime?))
                {
                    if (prop.CurrentValue is DateTime ndt)
                        prop.CurrentValue = ToUtcKind(ndt);
                }
            }
        }
    }

    public override int SaveChanges()
    {
        EnsureDateTimesAreUtc();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureDateTimesAreUtc();
        return await base.SaveChangesAsync(cancellationToken);
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