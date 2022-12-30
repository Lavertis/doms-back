using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;

public class GetAppointmentsByUserHandler
    : IRequestHandler<GetAppointmentsByUserQuery, HttpResult<PagedResponse<AppointmentResponse>>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetAppointmentsByUserHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<AppointmentResponse>>> Handle(
        GetAppointmentsByUserQuery request, CancellationToken cancellationToken)
    {
        var result = request.RoleName switch
        {
            Roles.Doctor => GetDoctorAppointmentsPagedResult(request.UserId, request.PaginationFilter),
            Roles.Patient => GetPatientAppointmentsPagedResult(request.UserId, request.PaginationFilter),
            _ => new HttpResult<PagedResponse<AppointmentResponse>>()
                .WithError(new Error {Message = "Invalid role"})
                .WithStatusCode(StatusCodes.Status400BadRequest)
        };
        return Task.FromResult(result);
    }

    private HttpResult<PagedResponse<AppointmentResponse>> GetDoctorAppointmentsPagedResult(
        Guid doctorId, PaginationFilter? paginationFilter)
    {
        var appointments = _appointmentRepository.GetAll()
            .Include(appointment => appointment.Doctor)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Type)
            .Include(appointment => appointment.Status)
            .Where(a => a.Doctor.Id == doctorId)
            .OrderBy(a => a.Date);

        var appointmentResponsesQueryable = appointments
            .Select(appointment => _mapper.Map<AppointmentResponse>(appointment));

        return PaginationUtils.CreatePagedHttpResult(appointmentResponsesQueryable, paginationFilter);
    }

    private HttpResult<PagedResponse<AppointmentResponse>> GetPatientAppointmentsPagedResult(
        Guid patientId, PaginationFilter? paginationFilter)
    {
        var appointments = _appointmentRepository.GetAll()
            .Include(appointment => appointment.Doctor)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Type)
            .Include(appointment => appointment.Status)
            .Where(a => a.Patient.Id == patientId)
            .OrderBy(a => a.Date);

        var appointmentResponsesQueryable = appointments
            .Select(appointment => _mapper.Map<AppointmentResponse>(appointment));

        return PaginationUtils.CreatePagedHttpResult(appointmentResponsesQueryable, paginationFilter);
    }
}