using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Appointments.CreateAppointment;

public class CreateAppointmentCommand : IRequest<HttpResult<AppointmentResponse>>
{
    public readonly DateTime Date;
    public readonly string Description;
    public readonly Guid DoctorId;
    public readonly Guid PatientId;
    public readonly string Type;

    public CreateAppointmentCommand(CreateAppointmentRequest request)
    {
        Date = request.Date;
        Description = request.Description;
        PatientId = request.PatientId;
        DoctorId = request.DoctorId;
        Type = request.Type;
    }

    public string RoleName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public Guid UserId { get; set; }
}