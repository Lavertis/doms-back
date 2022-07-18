using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Repositories.Repository;

namespace DoctorsOfficeApi.Repositories.PatientRepository;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}