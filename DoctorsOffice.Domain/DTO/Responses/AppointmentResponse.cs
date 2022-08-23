namespace DoctorsOffice.Domain.DTO.Responses;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string Status { get; set; } = null!;
    public string Type { get; set; } = null!;
}