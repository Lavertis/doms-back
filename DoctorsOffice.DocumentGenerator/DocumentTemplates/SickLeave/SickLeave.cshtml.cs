namespace DoctorsOffice.DocumentGenerator.DocumentTemplates.SickLeave;

public class SickLeaveTemplateData
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DoctorFullName { get; set; } = null!;
    public string PatientFullName { get; set; } = null!;
    public string PatientNationalId { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string Purpose { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}