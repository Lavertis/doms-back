namespace DoctorsOffice.Domain.DTO.Requests;

public class CreatePrescriptionRequest
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public DateTime FulfillmentDeadline { get; set; }
    public IList<CreateDrugItemRequest> DrugItems { get; set; } = null!;
}