﻿using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdateAppointment;

public class UpdateAppointmentCommand : IRequest<AppointmentResponse>
{
    public readonly Guid AppointmentId;
    public readonly DateTime? Date;
    public readonly string? Description;
    public readonly string Role;
    public readonly string? Status;
    public readonly string? Type;
    public readonly Guid UserId;

    public UpdateAppointmentCommand(UpdateAppointmentRequest request, Guid appointmentId, Guid userId, string role)
    {
        AppointmentId = appointmentId;
        UserId = userId;
        Role = role;
        Date = request.Date;
        Description = request.Description;
        Type = request.Type;
        Status = request.Status;
    }
}