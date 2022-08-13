using System.Linq.Expressions;
using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Domain.Repositories;

public interface IPrescriptionRepository : IRepository<Prescription>
{
    IQueryable<Prescription> GetByPatientId(Guid patientId,
        params Expression<Func<Prescription, object>>[] navigationProperties);

    IQueryable<Prescription> GetByDoctorId(Guid doctorId,
        params Expression<Func<Prescription, object>>[] navigationProperties);

    Task UpdateDrugItemsAsync(Prescription prescription, IList<DrugItem> drugItems);
}