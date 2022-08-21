using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Domain.Repositories;

public interface IAppointmentTypeRepository : IRepository<AppointmentType>
{
    Task<AppointmentType?> GetByNameAsync(string name);

    Task<bool> ExistsByNameAsync(string name);
}