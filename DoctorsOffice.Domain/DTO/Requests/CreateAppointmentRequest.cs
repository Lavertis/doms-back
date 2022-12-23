namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateAppointmentRequest
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid TypeId { get; set; }
}