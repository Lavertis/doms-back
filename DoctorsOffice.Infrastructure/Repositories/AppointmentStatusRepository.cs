using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Repositories;

public class AppointmentStatusRepository : Repository<AppointmentStatus>, IAppointmentStatusRepository
{
    public AppointmentStatusRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<AppointmentStatus?> GetByNameAsync(string name)
    {
        var appointmentStatus = await DbContext.Set<AppointmentStatus>()
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(e => e.Name == name);

        return appointmentStatus;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await DbContext.AppointmentStatuses.AnyAsync(a => a.Name == name);
    }
}