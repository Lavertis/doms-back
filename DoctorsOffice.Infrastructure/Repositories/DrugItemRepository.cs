using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;

namespace DoctorsOffice.Infrastructure.Repositories;

public class DrugItemRepository : Repository<DrugItem>, IDrugItemRepository
{
    public DrugItemRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}