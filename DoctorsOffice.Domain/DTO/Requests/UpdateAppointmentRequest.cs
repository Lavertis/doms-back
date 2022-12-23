namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateAppointmentRequest
{
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public string? Interview { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendations { get; set; }
    public Guid? TypeId { get; set; }
    public Guid? StatusId { get; set; }
}