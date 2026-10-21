using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity>
    where TEntity : class
{
    protected readonly CricPulseDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected RepositoryBase(CricPulseDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    protected Task<TEntity?> FindByIdAsync(Guid id)
    {
        return DbSet.FindAsync(id).AsTask();
    }

    protected Task AddEntityAsync(TEntity entity)
    {
        return DbSet.AddAsync(entity).AsTask();
    }

    protected void UpdateEntity(TEntity entity)
    {
        DbSet.Update(entity);
    }

    protected void RemoveEntity(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}