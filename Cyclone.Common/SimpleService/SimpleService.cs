// Cyclone.Common.SimpleService/SimpleService.cs (обновленная версия)

using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleResponse;
using Cyclone.Common.SimpleSoftDelete;
using Cyclone.Common.SimpleSoftDelete.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using DbUpdateException = Microsoft.EntityFrameworkCore.DbUpdateException;

namespace Cyclone.Common.SimpleService;

public class SimpleService<TEntity, TDbContext>(
    TDbContext db, 
    ILogger logger)
    where TEntity : BaseEntity 
    where TDbContext : SimpleDbContext
{
    protected readonly TDbContext Db = db;

    protected async Task<Response<TEntity>> CreateAsync(TEntity entity)
    {
        using var _ = LogContext.PushProperty("EntityType", typeof(TEntity).Name);
        using var __ = LogContext.PushProperty("EntityId", entity.Id);
        
        logger.Information("Creating entity {EntityType}", typeof(TEntity).Name);
        
        await Db.Set<TEntity>().AddAsync(entity);
        try
        {
            await Db.SaveChangesAsync();
            logger.Information("Successfully created entity {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, entity.Id);
            return entity;
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Failed to create entity {EntityType}", typeof(TEntity).Name);
            return "Ошибка сохранения: " + dbEx.Message;
        }
    }

    protected async Task<Response<TEntity>> UpdateAsync(TEntity entity)
    {
        using var _ = LogContext.PushProperty("EntityType", typeof(TEntity).Name);
        using var __ = LogContext.PushProperty("EntityId", entity.Id);
        
        logger.Information("Updating entity {EntityType} with ID {EntityId}", 
            typeof(TEntity).Name, entity.Id);
        
        try
        {
            Db.Set<TEntity>().Update(entity);
            await Db.SaveChangesAsync();
            logger.Information("Successfully updated entity {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, entity.Id);
            return entity;
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Failed to update entity {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, entity.Id);
            return "Ошибка сохранения: " + dbEx.Message;
        }
    }

    protected async Task<Response<List<EntityDeletionInfo>>> SoftDeleteAsync(TEntity entity)
    {
        using var _ = LogContext.PushProperty("EntityType", typeof(TEntity).Name);
        using var __ = LogContext.PushProperty("EntityId", entity.Id);

        logger.Information("Soft deleting entity {EntityType} with ID {EntityId}",
            typeof(TEntity).Name, entity.Id);

        var strategy = Db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            var deletedCount = await Db.Set<TEntity>().SoftDeleteCascadeAsync(entity);

            try
            {
                await Db.SaveChangesAsync();
                logger.Information(
                    "Successfully soft deleted entity {EntityType} with ID {EntityId}. Cascade deleted {DeletedCount} related entities",
                    typeof(TEntity).Name, entity.Id, deletedCount.Count);
                return Response<List<EntityDeletionInfo>>.Ok(deletedCount);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex, "Failed to soft delete entity {EntityType} with ID {EntityId}",
                    typeof(TEntity).Name, entity.Id);
                return "Ошибка сохранения: " + ex.Message;
            }
        });
    }

    protected async Task<Response<List<EntityDeletionInfo>>> RestoreAsync(TEntity entity)
    {
        using var _ = LogContext.PushProperty("EntityType", typeof(TEntity).Name);
        using var __ = LogContext.PushProperty("EntityId", entity.Id);
        
        if (!entity.IsDeleted) return $"Сущность с id--{ entity.Id } не удалена";
    
        var strategy = Db.Database.CreateExecutionStrategy();
    
        return await strategy.ExecuteAsync(async () =>
        {
            var restoredEntities = await Db.Set<TEntity>().RestoreCascadeAsync(entity);

            try
            {
                await Db.SaveChangesAsync();
                logger.Information(
                    "Successfully restore entity {EntityType} with ID {EntityId}. Cascade restore {RestoreCount} related entities",
                    typeof(TEntity).Name, entity.Id, restoredEntities);
                return Response<List<EntityDeletionInfo>>.Ok(restoredEntities);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex, "Failed to restore entity {EntityType} with ID {EntityId}",
                    typeof(TEntity).Name, entity.Id);
                return "Ошибка сохранения: " + ex.Message;
            }
        });
    }

    public async Task<Response<List<EntityDeletionInfo>>> DeleteAsync(string? id)
    {
        var findResult = await Db.FindByStringAsync<TEntity>(id);
        if (findResult.Failure)
            return Response<List<EntityDeletionInfo>>.Fail(findResult.Message, findResult.Errors.ToArray());
        var entity = findResult.Data!;
        
        return await SoftDeleteAsync(entity);
    }
    
}
