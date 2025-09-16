using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleSoftDelete.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Cyclone.Common.SimpleService;

public class SimpleService<TEntity, TDbContext>(TDbContext db)
    where TEntity : BaseEntity 
    where TDbContext : SimpleDbContext
{
    protected readonly TDbContext Db = db;

    protected async Task<Response<TEntity>> CreateAsync(TEntity entity)
    {
        await Db.Set<TEntity>().AddAsync(entity);
        try
        {
            await Db.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException dbEx)
        {
            return "Ошибка сохранения: " + dbEx.Message;
        }
    }

    protected async Task<Response<TEntity>> UpdateAsync(TEntity entity)
    {
        try
        {
            Db.Set<TEntity>().Update(entity);
            await Db.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException dbEx)
        {
            return "Ошибка сохранения: " + dbEx.Message;
        }
    }

    protected async Task<Response<int>> SoftDeleteAsync(TEntity entity)
    {
        var strategy = Db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            var deletedCount = await Db.Set<TEntity>().SoftDeleteCascadeAsync(entity);

            try
            {
                await Db.SaveChangesAsync();
                return Response<int>.Ok(deletedCount);
            }
            catch (DbUpdateException ex)
            {
                return "Ошибка сохранения: " + ex.Message;
            }
        });
    }
    
    protected async Task<Response<int>> RestoreAsync(TEntity entity)
    {
        if (!entity.IsDeleted) return $"Сущность с id--{ entity.Id } не удалена";
        
        var strategy = Db.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            var restoredEntities = await Db.Set<TEntity>().RestoreCascadeAsync(entity);

            try
            {
                await Db.SaveChangesAsync();
                return Response<int>.Ok(restoredEntities);
            }
            catch (DbUpdateException ex)
            {
                return "Ошибка сохранения: " + ex.Message;
            }
        });
    }
}