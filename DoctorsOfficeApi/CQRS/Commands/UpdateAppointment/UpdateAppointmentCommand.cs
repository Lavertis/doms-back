using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdateAppointment;

public class UpdateAppointmentCommand : IRequest<AppointmentResponse>
{
    public long AppointmentId { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; } = default!;
    public string? Type { get; set; } = default!;
    public string? Status { get; set; } = default!;

    public UpdateAppointmentCommand()
    {
    }

    public UpdateAppointmentCommand(long appointmentId, UpdateAppointmentRequest updateAppointmentRequest)
    {
        AppointmentId = appointmentId;
        Date = updateAppointmentRequest.Date;
        Description = updateAppointmentRequest.Description;
        Type = updateAppointmentRequest.Type;
        Status = updateAppointmentRequest.Status;
    }

    public UpdateAppointmentCommand(long appointmentId, DateTime? date, string? description, string? type, string? status)
    {
        AppointmentId = appointmentId;
        Date = date;
        Description = description;
        Type = type;
        Status = status;
    }
}