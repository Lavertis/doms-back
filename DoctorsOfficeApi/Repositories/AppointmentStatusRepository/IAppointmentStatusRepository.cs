using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Repositories.Repository;

namespace DoctorsOfficeApi.Repositories.AppointmentStatusRepository;

public interface IAppointmentStatusRepository : IRepository<AppointmentStatus>
{
    Task<AppointmentStatus> GetByNameAsync(string name);
    Task<AppointmentStatus?> GetByNameOrDefaultAsync(string name);

    Task<bool> ExistsByNameAsync(string name);
}