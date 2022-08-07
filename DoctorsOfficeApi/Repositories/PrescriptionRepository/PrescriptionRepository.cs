using System.Linq.Expressions;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Repositories.PrescriptionRepository;

public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public IQueryable<Prescription> GetByPatientId(Guid patientId,
        params Expression<Func<Prescription, object>>[] navigationProperties)
    {
        var prescriptionsQueryable = DbContext.Prescriptions
            .Where(p => p.PatientId == patientId);

        return navigationProperties.Aggregate(
            prescriptionsQueryable,
            (current, prop) => current.Include(prop)
        );
    }

    public IQueryable<Prescription> GetByDoctorId(Guid doctorId,
        params Expression<Func<Prescription, object>>[] navigationProperties)
    {
        var prescriptionsQueryable = DbContext.Prescriptions
            .Where(p => p.DoctorId == doctorId);

        return navigationProperties.Aggregate(
            prescriptionsQueryable,
            (current, prop) => current.Include(prop)
        );
    }

    public override Task<Prescription> UpdateByIdAsync(Guid id, Prescription prescription)
    {
        DbContext.Prescriptions.Attach(prescription);
        return base.UpdateByIdAsync(id, prescription);
    }

    public async Task UpdateDrugItemsAsync(Prescription prescription, IList<DrugItem> drugItems)
    {
        DbContext.Prescriptions.Attach(prescription);
        DbContext.DrugItems.AttachRange(drugItems);

        DbContext.DrugItems.RemoveRange(prescription.DrugItems);
        DbContext.DrugItems.AddRange(drugItems);
        prescription.DrugItems = drugItems;

        await DbContext.SaveChangesAsync();
    }
}