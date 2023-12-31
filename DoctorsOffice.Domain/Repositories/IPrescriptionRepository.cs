using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Domain.Repositories;

public interface IPrescriptionRepository : IRepository<Prescription>
{
    Task UpdateDrugItemsAsync(Prescription prescription, List<DrugItem> drugItems);
}