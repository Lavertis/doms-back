using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Database;

namespace DoctorsOffice.Infrastructure.Repositories;

public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(AppDbContext dbContext) : base(dbContext)
    {
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