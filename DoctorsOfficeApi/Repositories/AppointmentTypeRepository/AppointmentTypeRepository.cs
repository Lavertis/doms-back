using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Repositories.AppointmentTypeRepository;

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
        return await DbContext.Set<AppointmentType>()
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(e => e.Name == name);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await DbContext.AppointmentTypes.AnyAsync(a => a.Name == name);
    }
}