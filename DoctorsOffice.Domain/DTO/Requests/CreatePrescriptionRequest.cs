namespace DoctorsOffice.Domain.DTO.Requests;

public class CreatePrescriptionRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid PatientId { get; set; }
    public IList<Guid> DrugsIds { get; set; } = null!;
}