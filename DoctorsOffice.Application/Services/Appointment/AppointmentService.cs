﻿using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Exceptions;

namespace DoctorsOffice.Application.Services.Appointment;

public class AppointmentService : IAppointmentService
{
    public bool CanUserAccessAppointment(Guid userId, string role, Guid appointmentDoctorId, Guid appointmentPatientId)
    {
        return role switch
        {
            RoleTypes.Patient when appointmentPatientId != userId =>
                throw new ForbiddenException("Trying to get appointment of another doctor"),
            RoleTypes.Doctor when appointmentDoctorId != userId =>
                throw new ForbiddenException("Trying to get appointment of another patient"),
            RoleTypes.Patient or RoleTypes.Doctor or RoleTypes.Admin => true,
            _ => false
        };
    }
}