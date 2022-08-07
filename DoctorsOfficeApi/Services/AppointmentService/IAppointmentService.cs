namespace DoctorsOfficeApi.Services.AppointmentService;

public interface IAppointmentService
{
    bool CanUserAccessAppointment(Guid userId, string role, Guid appointmentDoctorId, Guid appointmentPatientId);
}