using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;

public class GetAppointmentsByUserHandler
    : IRequestHandler<GetAppointmentsByUserQuery, HttpResult<IEnumerable<AppointmentResponse>>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByUserHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<HttpResult<IEnumerable<AppointmentResponse>>> Handle(
        GetAppointmentsByUserQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<AppointmentResponse>>();

        return request.Role switch
        {
            RoleTypes.Doctor => result.WithValue(await GetDoctorAppointments(request.UserId, cancellationToken)),
            RoleTypes.Patient => result.WithValue(await GetPatientAppointments(request.UserId, cancellationToken)),
            _ => result.WithError(new Error {Message = "Invalid role"}).WithStatusCode(StatusCodes.Status400BadRequest)
        };
    }

    private async Task<IList<AppointmentResponse>> GetDoctorAppointments(
        Guid doctorId, CancellationToken cancellationToken)
    {
        var appointments = _appointmentRepository.GetAll(
                a => a.Doctor,
                a => a.Patient,
                a => a.Type,
                a => a.Status)
            .Where(a => a.Doctor.Id == doctorId)
            .OrderBy(a => a.Date);

        return await appointments.Select(appointment => new AppointmentResponse(appointment))
            .ToListAsync(cancellationToken: cancellationToken);
    }

    private async Task<IList<AppointmentResponse>> GetPatientAppointments(
        Guid patientId, CancellationToken cancellationToken)
    {
        var appointments = _appointmentRepository.GetAll(
                a => a.Doctor,
                a => a.Patient,
                a => a.Type,
                a => a.Status)
            .Where(a => a.Patient.Id == patientId)
            .OrderBy(a => a.Date);

        return await appointments.Select(appointment => new AppointmentResponse(appointment))
            .ToListAsync(cancellationToken: cancellationToken);
    }
}