using DoctorsOfficeApi.Entities;

namespace DoctorsOfficeApi.Models.Responses;

public class PrescriptionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public IList<Guid> DrugsIds { get; set; } = default!; // TODO replace with lift of drugResponses

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
}