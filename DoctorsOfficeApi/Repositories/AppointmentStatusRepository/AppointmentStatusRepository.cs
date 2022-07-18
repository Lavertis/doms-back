using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Repositories.AppointmentStatusRepository;

public class AppointmentStatusRepository : Repository<AppointmentStatus>, IAppointmentStatusRepository
{
    public AppointmentStatusRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<AppointmentStatus> GetByNameAsync(string name)
    {
        var appointmentStatus = await GetByNameOrDefaultAsync(name);
        if (appointmentStatus is null)
        {
            throw new NotFoundException($"AppointmentStatus with name: {name} not found");
        }

        return appointmentStatus;
    }

    public async Task<AppointmentStatus?> GetByNameOrDefaultAsync(string name)
    {
        return await DbContext.Set<AppointmentStatus>()
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(e => e.Name == name);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await DbContext.AppointmentStatuses.AnyAsync(a => a.Name == name);
    }
}