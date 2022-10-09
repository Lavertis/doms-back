using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Appointments.UpdateAppointment;

public class UpdateAppointmentCommand : IRequest<HttpResult<AppointmentResponse>>
{
    public readonly DateTime? Date;
    public readonly string? Description;
    public readonly string? Diagnosis;
    public readonly string? Interview;
    public readonly string? Recommendations;
    public readonly string? Status;
    public readonly string? Type;

    public UpdateAppointmentCommand(UpdateAppointmentRequest request)
    {
        Date = request.Date;
        Description = request.Description;
        Type = request.Type;
        Status = request.Status;
        Interview = request.Interview;
        Diagnosis = request.Diagnosis;
        Recommendations = request.Recommendations;
    }

    public string RoleName { get; set; } = null!;
    public Guid AppointmentId { get; set; }
    public Guid UserId { get; set; }
}