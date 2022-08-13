namespace DoctorsOffice.Domain.DTO.Requests;

public class GetAppointmentsFilteredRequest
{
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? DoctorId { get; set; }
}