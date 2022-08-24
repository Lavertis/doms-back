using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> GetAll();
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<bool> DeleteByIdAsync(Guid id);
    Task<bool> ExistsByIdAsync(Guid id);
}