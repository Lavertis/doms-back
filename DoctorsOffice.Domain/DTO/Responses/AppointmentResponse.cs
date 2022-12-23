namespace DoctorsOffice.Domain.DTO.Responses;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public Guid StatusId { get; set; }
    public Guid TypeId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public string Interview { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string Recommendations { get; set; } = null!;
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
}