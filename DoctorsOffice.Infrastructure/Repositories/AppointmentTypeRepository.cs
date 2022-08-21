using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Repositories;

public class AppointmentTypeRepository : Repository<AppointmentType>, IAppointmentTypeRepository
{
    public AppointmentTypeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<AppointmentType?> GetByNameAsync(string name)
    {
        var appointmentType = await DbContext.Set<AppointmentType>()
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(e => e.Name == name);

        return appointmentType;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await DbContext.AppointmentTypes.AnyAsync(a => a.Name == name);
    }
}