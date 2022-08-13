using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Exceptions;
using DoctorsOffice.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.GetAppointmentsByUser;

public class GetAppointmentsByUserHandler : IRequestHandler<GetAppointmentsByUserQuery, IList<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByUserHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IList<AppointmentResponse>> Handle(GetAppointmentsByUserQuery request,
        CancellationToken cancellationToken)
    {
        return request.Role switch
        {
            RoleTypes.Doctor => await GetDoctorAppointments(request.UserId, cancellationToken),
            RoleTypes.Patient => await GetPatientAppointments(request.UserId, cancellationToken),
            _ => throw new BadRequestException("Invalid role")
        };
    }

    private async Task<IList<AppointmentResponse>> GetDoctorAppointments(
        Guid doctorId,
        CancellationToken cancellationToken)
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
        Guid patientId,
        CancellationToken cancellationToken)
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