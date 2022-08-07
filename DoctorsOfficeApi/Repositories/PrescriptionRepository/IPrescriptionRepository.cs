using System.Linq.Expressions;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Repositories.Repository;

namespace DoctorsOfficeApi.Repositories.PrescriptionRepository;

public interface IPrescriptionRepository : IRepository<Prescription>
{
    IQueryable<Prescription> GetByPatientId(Guid patientId,
        params Expression<Func<Prescription, object>>[] navigationProperties);

    IQueryable<Prescription> GetByDoctorId(Guid doctorId,
        params Expression<Func<Prescription, object>>[] navigationProperties);

    Task UpdateDrugItemsAsync(Prescription prescription, IList<DrugItem> drugItems);
}