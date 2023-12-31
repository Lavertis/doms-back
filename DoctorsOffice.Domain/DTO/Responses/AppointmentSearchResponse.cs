namespace DoctorsOffice.Domain.DTO.Responses;

public class AppointmentSearchResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public Guid PatientId { get; set; }
    public string PatientFirstName { get; set; } = null!;
    public string PatientLastName { get; set; } = null!;
    public string PatientEmail { get; set; } = null!;
    public string PatientPhoneNumber { get; set; } = null!;
    public string DoctorFirstName { get; set; } = null!;
    public string DoctorLastName { get; set; } = null!;
    public Guid StatusId { get; set; }
    public Guid TypeId { get; set; }
}