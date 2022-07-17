using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;

namespace DoctorsOfficeApi.Services.PatientService;

public class PatientService : IPatientService
{
    private readonly AppDbContext _dbContext;


    public PatientService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Patient> GetPatientByIdAsync(Guid id)
    {
        var patient = await _dbContext.Patients.FindAsync(id);
        if (patient is null)
            throw new NotFoundException("Patient not found");

        return patient;
    }
}