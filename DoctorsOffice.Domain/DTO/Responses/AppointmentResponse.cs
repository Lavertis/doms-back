using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Domain.DTO.Responses;

public class AppointmentResponse
{
    public AppointmentResponse(Appointment appointment)
    {
        Id = appointment.Id;
        Date = appointment.Date;
        Description = appointment.Description;
        PatientId = appointment.PatientId;
        DoctorId = appointment.DoctorId;
        Status = appointment.Status.Name;
        Type = appointment.Type.Name;
    }

    public AppointmentResponse()
    {
    }

    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}