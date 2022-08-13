using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Domain.DTO.Responses;

public class PrescriptionResponse
{
    public PrescriptionResponse()
    {
    }

    public PrescriptionResponse(Prescription prescription)
    {
        Id = prescription.Id;
        Title = prescription.Title;
        Description = prescription.Description;
        DoctorId = prescription.DoctorId;
        PatientId = prescription.PatientId;
        DrugsIds = prescription.DrugItems.Select(d => d.Id).ToList();
    }

    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public IList<Guid> DrugsIds { get; set; } = default!; // TODO replace with list of drugResponses
}