namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateAppointmentRequest
{
    public DateTime? Date { get; set; }
    public string? Description { get; set; } = default!;
    public string? Type { get; set; } = default!;
    public string? Status { get; set; } = default!;
}