using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Utils;

namespace DoctorsOffice.Application.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    public CommonResult<bool> CanUserAccessAppointment(
        Guid userId, string role, Guid appointmentDoctorId, Guid appointmentPatientId)
    {
        var result = new CommonResult<bool>();

        return role switch
        {
            RoleTypes.Patient when appointmentPatientId != userId
                => result.WithError(new Error {Message = "Trying to get appointment of another doctor"}),
            RoleTypes.Doctor when appointmentDoctorId != userId
                => result.WithError(new Error {Message = "Trying to get appointment of another patient"}),
            RoleTypes.Patient or RoleTypes.Doctor or RoleTypes.Admin
                => result.WithValue(true),
            _
                => result.WithValue(false)
        };
    }
}