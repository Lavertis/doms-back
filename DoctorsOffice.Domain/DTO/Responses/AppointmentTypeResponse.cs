namespace DoctorsOffice.Domain.DTO.Responses;

public class AppointmentTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ushort DurationMinutes { get; set; }
}