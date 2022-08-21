using DoctorsOffice.Domain.Utils;

namespace DoctorsOffice.Application.Services.Appointments;

public interface IAppointmentService
{
    CommonResult<bool> CanUserAccessAppointment(
        Guid userId, string role, Guid appointmentDoctorId, Guid appointmentPatientId);
}