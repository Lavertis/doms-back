namespace DoctorsOffice.Domain.DTO.Requests;

public class CreatePrescriptionRequest
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid PatientId { get; set; }
    public IList<Guid> DrugsIds { get; set; } = default!;
}