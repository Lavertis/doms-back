using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;

namespace DoctorsOffice.Infrastructure.Repositories;

public class SickLeaveRepository : Repository<SickLeave>, ISickLeaveRepository
{
    public SickLeaveRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}