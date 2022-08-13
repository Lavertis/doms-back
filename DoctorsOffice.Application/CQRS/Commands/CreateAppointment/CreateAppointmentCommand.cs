using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.CreateAppointment;

public class CreateAppointmentCommand : IRequest<AppointmentResponse>
{
    public readonly DateTime Date;
    public readonly string Description;
    public readonly Guid DoctorId;
    public readonly Guid PatientId;
    public readonly string Role;
    public readonly string Status;
    public readonly string Type;
    public readonly Guid UserId;

    public CreateAppointmentCommand(CreateAppointmentRequest request, string status, string role, Guid userId)
    {
        Status = status;
        Role = role;
        UserId = userId;
        Date = request.Date;
        Description = request.Description;
        PatientId = request.PatientId;
        DoctorId = request.DoctorId;
        Type = request.Type;
    }
}