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

    public virtual IQueryable<TEntity> GetAll()
    {
        return DbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await DbContext.Set<TEntity>()
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        var createdEntity = (await DbContext.Set<TEntity>().AddAsync(entity)).Entity;
        await DbContext.SaveChangesAsync();
        return createdEntity;
    }

    public virtual async Task<IEnumerable<TEntity>> CreateRangeAsync(List<TEntity> entities)
    {
        entities.ForEach(e => e.Id = e.Id == Guid.Empty ? Guid.NewGuid() : e.Id);
        await DbContext.Set<TEntity>().AddRangeAsync(entities);
        await DbContext.SaveChangesAsync();
        return entities;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
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