namespace DoctorsOffice.Domain.DTO.Responses;

public class TimetableResponse
{
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public Guid DoctorId { get; set; }
}