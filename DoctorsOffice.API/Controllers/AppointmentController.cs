using DoctorsOffice.Application.CQRS.Commands.Appointments.CreateAppointment;
using DoctorsOffice.Application.CQRS.Commands.Appointments.UpdateAppointment;
using DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentById;
using DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;
using DoctorsOffice.Application.CQRS.Queries.Appointments.GetFilteredAppointments;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : BaseController
{
    public AppointmentController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all appointments for authenticated user sorted by date. For doctors and patients.
    /// </summary>
    [HttpGet("user/current")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAllAppointmentsForAuthenticatedUserAsync()
        => CreateResponse(await Mediator.Send(new GetAppointmentsByUserQuery
        {
            UserId = JwtSubject(),
            RoleName = JwtRole()
        }));

    /// <summary>
    /// Returns appointment by id. User can only get owned appointment. For doctors and patients
    /// </summary>
    [HttpGet("user/current/{appointmentId:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentByIdAsync(Guid appointmentId)
        => CreateResponse(await Mediator.Send(new GetAppointmentByIdQuery
        {
            AppointmentId = appointmentId,
            UserId = JwtSubject(),
            RoleName = JwtRole()
        }));

    /// <summary>
    /// Returns all appointments matching search criteria sorted by date. Only for doctors
    /// </summary>
    [HttpGet("doctor/current/search")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<PagedResponse<AppointmentSearchResponse>>> GetAppointmentsFilteredAsync(
        DateTime? dateStart,
        DateTime? dateEnd,
        Guid? patientId,
        string? type,
        string? status,
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetFilteredAppointmentsQuery(paginationFilter)
        {
            DateStart = dateStart,
            DateEnd = dateEnd,
            Type = type,
            Status = status,
            PatientId = patientId,
            DoctorId = JwtSubject()
        }));

    /// <summary>
    /// Returns appointments for authenticated patient matching search criteria sorted by date. Only for patients
    /// </summary>
    [HttpGet("patient/current/search")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<PagedResponse<AppointmentSearchResponse>>>
        GetAppointmentsForAuthenticatedPatientFilteredAsync(
            DateTime? dateStart,
            DateTime? dateEnd,
            string? type,
            string? status,
            [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetFilteredAppointmentsQuery(paginationFilter)
        {
            DateStart = dateStart,
            DateEnd = dateEnd,
            Type = type,
            Status = status,
            PatientId = JwtSubject()
        }));

    /// <summary>
    /// Creates new appointment. Only for doctors
    /// </summary>
    [HttpPost("doctor/current")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request)
        => CreateResponse(await Mediator.Send(new CreateAppointmentCommand(request)
        {
            RoleName = RoleTypes.Doctor,
            UserId = JwtSubject(),
            Status = AppointmentStatuses.Accepted
        }));

    /// <summary>
    /// Creates appointment request. Only for patients
    /// </summary>
    [HttpPost("patient/current/request")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentRequestAsync(CreateAppointmentRequest request)
        => CreateResponse(await Mediator.Send(new CreateAppointmentCommand(request)
        {
            RoleName = RoleTypes.Patient,
            UserId = JwtSubject(),
            Status = AppointmentStatuses.Pending
        }));

    /// <summary>
    /// Updates appointment. For patients and doctors
    /// </summary>
    [HttpPatch("user/current/{appointmentId:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointmentByIdAsync(
        UpdateAppointmentRequest request, Guid appointmentId)
        => CreateResponse(await Mediator.Send(new UpdateAppointmentCommand(request)
        {
            AppointmentId = appointmentId,
            UserId = JwtSubject(),
            RoleName = JwtRole()
        }));
}