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
    public readonly Guid TypeId;

    public CreateAppointmentCommand(CreateAppointmentRequest request)
    {
        Date = request.Date;
        Description = request.Description;
        PatientId = request.PatientId;
        DoctorId = request.DoctorId;
        TypeId = request.TypeId;
    }

    public string RoleName { get; set; } = null!;
    public Guid StatusId { get; set; }
    public Guid UserId { get; set; }
}