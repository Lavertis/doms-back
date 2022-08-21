using System.Linq.Expressions;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly AppDbContext DbContext;

    protected Repository(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includeFields)
    {
        var entitiesQueryable = DbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        return includeFields.Aggregate(
            entitiesQueryable,
            (current, prop) => current.Include(prop)
        );
    }

    public virtual async Task<TEntity?> GetByIdAsync(
        Guid id, params Expression<Func<TEntity, object>>[] includeFields)
    {
        var entitiesQueryable = DbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        entitiesQueryable = includeFields.Aggregate(
            entitiesQueryable,
            (current, prop) => current.Include(prop)
        );

        var entity = await entitiesQueryable.FirstOrDefaultAsync(e => e.Id == id);
        return entity;
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        var createdEntity = (await DbContext.Set<TEntity>().AddAsync(entity)).Entity;
        await DbContext.SaveChangesAsync();
        return createdEntity;
    }

    public virtual async Task<TEntity> UpdateByIdAsync(Guid id, TEntity entity)
    {
        entity.Id = id;
        var updatedEntity = DbContext.Set<TEntity>().Update(entity).Entity;

        await DbContext.SaveChangesAsync();
        return updatedEntity;
    }

    public virtual async Task<bool> DeleteByIdAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null)
            return false;
        DbContext.Set<TEntity>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await DbContext.Set<TEntity>().FindAsync(id) is not null;
    }
}