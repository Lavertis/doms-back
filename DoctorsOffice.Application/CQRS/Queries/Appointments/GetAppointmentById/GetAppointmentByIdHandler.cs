using DoctorsOffice.Application.Services.Appointments;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentById;

public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, HttpResult<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentService _appointmentService;

    public GetAppointmentByIdHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentService appointmentService)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentService = appointmentService;
    }

    public async Task<HttpResult<AppointmentResponse>> Handle(
        GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AppointmentResponse>();

        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId,
            a => a.Doctor,
            a => a.Patient,
            a => a.Type,
            a => a.Status);
        if (appointment is null)
        {
            return result
                .WithError(new Error {Message = $"Appointment with id {request.AppointmentId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var userCanAccessAppointment = _appointmentService.CanUserAccessAppointment(
            userId: request.UserId,
            role: request.RoleName,
            appointmentDoctorId: appointment.DoctorId,
            appointmentPatientId: appointment.PatientId
        );

        if (userCanAccessAppointment.IsFailed)
            return result
                .WithError(userCanAccessAppointment.Error)
                .WithStatusCode(StatusCodes.Status403Forbidden);

        if (userCanAccessAppointment.IsSuccess && userCanAccessAppointment.Value)
            return result.WithValue(new AppointmentResponse(appointment));

        return result
            .WithError(new Error {Message = "Something went wrong"})
            .WithStatusCode(StatusCodes.Status500InternalServerError);
    }
}