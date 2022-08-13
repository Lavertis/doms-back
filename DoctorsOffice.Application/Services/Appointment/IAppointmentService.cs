namespace DoctorsOffice.Application.Services.Appointment;

public interface IAppointmentService
{
    bool CanUserAccessAppointment(Guid userId, string role, Guid appointmentDoctorId, Guid appointmentPatientId);
}