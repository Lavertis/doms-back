namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdatePrescriptionRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? PatientId { get; set; }
    public IList<Guid>? DrugIds { get; set; }
}