using System.Linq.Expressions;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Repositories.Repository;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly AppDbContext DbContext;

    public Repository(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] navigationProperties)
    {
        var adminsQueryable = DbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        return navigationProperties.Aggregate(
            adminsQueryable,
            (current, prop) => current.Include(prop)
        );
    }

    public virtual async Task<TEntity?> GetByIdOrDefaultAsync(
        Guid id,
        params Expression<Func<TEntity, object>>[] navigationProperties)
    {
        var adminsQueryable = DbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        adminsQueryable = navigationProperties.Aggregate(
            adminsQueryable,
            (current, prop) => current.Include(prop)
        );

        return await adminsQueryable.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<TEntity> GetByIdAsync(
        Guid id,
        params Expression<Func<TEntity, object>>[] navigationProperties)
    {
        var entity = await GetByIdOrDefaultAsync(id, navigationProperties);
        if (entity == null)
            throw new NotFoundException($"{typeof(TEntity).Name} with id {id} not found");

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

    public virtual async Task DeleteByIdAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);

        DbContext.Set<TEntity>().Remove(entity);
        await DbContext.SaveChangesAsync();
    }
}