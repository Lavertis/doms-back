using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AppointmentRepository;
using DoctorsOfficeApi.Services.AppointmentService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;

public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentResponse>
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

    public async Task<AppointmentResponse> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId,
            a => a.Doctor,
            a => a.Patient,
            a => a.Type,
            a => a.Status);

        var canUserAccessAppointment = _appointmentService.CanUserAccessAppointment(
            userId: request.UserId,
            role: request.RoleName,
            appointmentDoctorId: appointment.DoctorId,
            appointmentPatientId: appointment.PatientId
        );
        if (canUserAccessAppointment)
            return new AppointmentResponse(appointment);
        throw new AppException("Something went wrong");
    }
}