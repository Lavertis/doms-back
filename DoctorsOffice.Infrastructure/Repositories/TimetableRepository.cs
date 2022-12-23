using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;

namespace DoctorsOffice.Infrastructure.Repositories;

public class TimetableRepository : Repository<Timetable>, ITimetableRepository
{
    public TimetableRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}