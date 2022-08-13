using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Exceptions;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Repositories;

public class AppointmentTypeRepository : Repository<AppointmentType>, IAppointmentTypeRepository
{
    public AppointmentTypeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<AppointmentType> GetByNameAsync(string name)
    {
        var appointmentType = await GetByNameOrDefaultAsync(name);
        if (appointmentType is null)
        {
            throw new NotFoundException($"AppointmentStatus with name: {name} not found");
        }

        return appointmentType;
    }

    public async Task<AppointmentType?> GetByNameOrDefaultAsync(string name)
    {
        return await DbContext.Set<Domain.Entities.AppointmentType>()
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(e => e.Name == name);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await DbContext.AppointmentTypes.AnyAsync(a => a.Name == name);
    }
}