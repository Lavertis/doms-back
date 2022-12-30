namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateSickLeaveRequest
{
    public Guid? PatientId { get; set; }

    public Guid? DoctorId { get; set; }

    public Guid? AppointmentId { get; set; }
    public DateTime? DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public string? Diagnosis { get; set; } = "";

    public string? Purpose { get; set; } = "";
}