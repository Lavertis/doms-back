using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Services.DoctorService;

public interface IDoctorService
{
    Task<Doctor> GetDoctorByIdAsync(Guid id);
}