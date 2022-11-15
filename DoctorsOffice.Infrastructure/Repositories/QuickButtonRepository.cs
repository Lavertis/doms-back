using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;

namespace DoctorsOffice.Infrastructure.Repositories;

public class QuickButtonRepository : Repository<QuickButton>, IQuickButtonRepository
{
    public QuickButtonRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}