namespace DoctorsOffice.Domain.DTO.Responses;

public class PrescriptionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public IList<Guid> DrugsIds { get; set; } = null!; // TODO replace with list of drugResponses
}