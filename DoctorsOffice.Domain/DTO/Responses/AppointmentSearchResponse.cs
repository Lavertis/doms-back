using DoctorsOffice.Domain.Entities;

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
    public string Status { get; set; } = null!;
    public string Type { get; set; } = null!;

    public AppointmentSearchResponse()
    {
    }
    
    public AppointmentSearchResponse(Appointment appointment)
    {
        Id = appointment.Id;
        Date = appointment.Date;
        Description = appointment.Description;
        PatientId = appointment.PatientId;
        PatientFirstName = appointment.Patient.FirstName;
        PatientLastName = appointment.Patient.LastName;
        PatientEmail = appointment.Patient.AppUser.Email;
        PatientPhoneNumber = appointment.Patient.AppUser.PhoneNumber;
        Status = appointment.Status.Name;
        Type = appointment.Type.Name;
    }
}