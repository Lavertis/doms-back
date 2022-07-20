using System.Linq.Expressions;
using DoctorsOfficeApi.Entities;

namespace DoctorsOfficeApi.Repositories.Repository;

public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] navigationProperties);

    Task<TEntity?> GetByIdOrDefaultAsync(Guid id, params Expression<Func<TEntity, object>>[] navigationProperties);
    Task<TEntity> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] navigationProperties);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateByIdAsync(Guid id, TEntity entity);
    Task DeleteByIdAsync(Guid id);
    Task<bool> ExistsByIdAsync(Guid id);
}