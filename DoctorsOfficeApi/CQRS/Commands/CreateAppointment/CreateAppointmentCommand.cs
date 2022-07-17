using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreateAppointment;

public class CreateAppointmentCommand : IRequest<AppointmentResponse>
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = default!;
    public Guid PatientId { get; set; } = default!;
    public Guid DoctorId { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;

    public CreateAppointmentCommand()
    {
    }

    public CreateAppointmentCommand(CreateAppointmentRequest createAppointmentRequest)
    {
        Date = createAppointmentRequest.Date;
        Description = createAppointmentRequest.Description;
        PatientId = Guid.Parse(createAppointmentRequest.PatientId);
        DoctorId = Guid.Parse(createAppointmentRequest.DoctorId);
        Type = createAppointmentRequest.Type;
    }
}