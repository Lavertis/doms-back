namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdatePrescriptionRequest
{
    public Guid? PatientId { get; set; }
    public DateTime? FulfillmentDeadline { get; set; }
    public IList<CreateDrugItemRequest>? DrugItems { get; set; }
}