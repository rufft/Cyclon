using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleSoftDelete;
using Microsoft.EntityFrameworkCore;

namespace Cyclone.Common.SimpleService;

public class SimpleService<TEntity, TTDbContext>(TTDbContext db)
    where TEntity : BaseEntity 
    where TTDbContext : SimpleDbContext
{
    protected readonly TTDbContext Db = db;

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