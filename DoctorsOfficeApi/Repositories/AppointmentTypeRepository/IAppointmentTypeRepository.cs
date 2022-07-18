using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Repositories.Repository;

namespace DoctorsOfficeApi.Repositories.AppointmentTypeRepository;

public interface IAppointmentTypeRepository : IRepository<AppointmentType>
{
    Task<AppointmentType> GetByNameAsync(string name);
    Task<AppointmentType?> GetByNameOrDefaultAsync(string name);

    Task<bool> ExistsByNameAsync(string name);
}