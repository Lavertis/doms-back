using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;

namespace DoctorsOfficeApi.Services.DoctorService;

public class DoctorService : IDoctorService
{
    private readonly AppDbContext _dbContext;

    public DoctorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Doctor> GetDoctorByIdAsync(Guid id)
    {
        var doctor = await _dbContext.Doctors.FindAsync(id);
        if (doctor is null)
            throw new NotFoundException("Doctor not found");

        return doctor;
    }
}