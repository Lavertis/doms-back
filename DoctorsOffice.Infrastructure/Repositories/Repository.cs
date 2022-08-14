using System.Linq.Expressions;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Exceptions;
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
        var adminsQueryable = DbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        return includeFields.Aggregate(
            adminsQueryable,
            (current, prop) => current.Include(prop)
        );
    }

    public virtual async Task<TEntity?> GetByIdOrDefaultAsync(
        Guid id,
        params Expression<Func<TEntity, object>>[] includeFields)
    {
        var adminsQueryable = DbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        adminsQueryable = includeFields.Aggregate(
            adminsQueryable,
            (current, prop) => current.Include(prop)
        );

        return await adminsQueryable.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<TEntity> GetByIdAsync(
        Guid id,
        params Expression<Func<TEntity, object>>[] includeFields)
    {
        var entity = await GetByIdOrDefaultAsync(id, includeFields);
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

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await DbContext.Set<TEntity>().FindAsync(id) is not null;
    }
}