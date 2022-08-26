using AutoMapper;
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
    private readonly IMapper _mapper;

    public GetAppointmentsByUserHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<AppointmentResponse>>> Handle(
        GetAppointmentsByUserQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<AppointmentResponse>>();

        return request.RoleName switch
        {
            RoleTypes.Doctor => result.WithValue(await GetDoctorAppointments(request.UserId, cancellationToken)),
            RoleTypes.Patient => result.WithValue(await GetPatientAppointments(request.UserId, cancellationToken)),
            _ => result.WithError(new Error {Message = "Invalid role"}).WithStatusCode(StatusCodes.Status400BadRequest)
        };
    }

    private async Task<IList<AppointmentResponse>> GetDoctorAppointments(
        Guid doctorId, CancellationToken cancellationToken)
    {
        var appointments = _appointmentRepository.GetAll()
            .Include(appointment => appointment.Doctor)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Type)
            .Include(appointment => appointment.Status)
            .Where(a => a.Doctor.Id == doctorId)
            .OrderBy(a => a.Date);

        var appointmentResponses = await appointments
            .Select(appointment => _mapper.Map<AppointmentResponse>(appointment))
            .ToListAsync(cancellationToken: cancellationToken);
        return appointmentResponses;
    }

    private async Task<IList<AppointmentResponse>> GetPatientAppointments(
        Guid patientId, CancellationToken cancellationToken)
    {
        var appointments = _appointmentRepository.GetAll()
            .Include(appointment => appointment.Doctor)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Type)
            .Include(appointment => appointment.Status)
            .Where(a => a.Patient.Id == patientId)
            .OrderBy(a => a.Date);

        var appointmentResponses = await appointments
            .Select(appointment => _mapper.Map<AppointmentResponse>(appointment))
            .ToListAsync(cancellationToken: cancellationToken);
        return appointmentResponses;
    }
}