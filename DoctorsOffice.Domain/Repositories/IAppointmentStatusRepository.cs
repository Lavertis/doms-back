using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Domain.Repositories;

public interface IAppointmentStatusRepository : IRepository<AppointmentStatus>
{
    Task<AppointmentStatus> GetByNameAsync(string name);
    Task<AppointmentStatus?> GetByNameOrDefaultAsync(string name);

    Task<bool> ExistsByNameAsync(string name);
}