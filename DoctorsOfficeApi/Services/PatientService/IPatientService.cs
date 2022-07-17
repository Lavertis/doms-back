using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Services.PatientService;

public interface IPatientService
{
    public Task<Patient> GetPatientByIdAsync(Guid id);
}