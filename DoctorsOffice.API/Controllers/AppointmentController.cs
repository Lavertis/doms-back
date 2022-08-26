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
        => CreateResponse(await Mediator.Send(new GetAppointmentsByUserQuery(userId: JwtSubject(), role: JwtRole())));

    /// <summary>
    /// Returns appointment by id. User can only get owned appointment. For doctors and patients
    /// </summary>
    [HttpGet("user/current/{appointmentId:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentByIdAsync(Guid appointmentId)
        => CreateResponse(await Mediator.Send(new GetAppointmentByIdQuery(
            appointmentId: appointmentId, userId: JwtSubject(), roleName: JwtRole()
        )));

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
        => CreateResponse(await Mediator.Send(new GetFilteredAppointmentsQuery(
            new GetAppointmentsFilteredRequest
            {
                DateStart = dateStart,
                DateEnd = dateEnd,
                Type = type,
                Status = status,
                PatientId = patientId,
                DoctorId = JwtSubject()
            }, paginationFilter)));

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
        => CreateResponse(await Mediator.Send(new GetFilteredAppointmentsQuery(
            new GetAppointmentsFilteredRequest
            {
                DateStart = dateStart,
                DateEnd = dateEnd,
                Type = type,
                Status = status,
                PatientId = JwtSubject()
            }, paginationFilter)));

    /// <summary>
    /// Creates new appointment. Only for doctors
    /// </summary>
    [HttpPost("doctor/current")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request)
        => CreateResponse(await Mediator.Send(new CreateAppointmentCommand(
            request: request,
            status: AppointmentStatuses.Accepted,
            role: RoleTypes.Doctor,
            userId: JwtSubject()
        )));

    /// <summary>
    /// Creates appointment request. Only for patients
    /// </summary>
    [HttpPost("patient/current/request")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentRequestAsync(CreateAppointmentRequest request)
        => CreateResponse(await Mediator.Send(new CreateAppointmentCommand(
            request: request,
            status: AppointmentStatuses.Pending,
            role: RoleTypes.Patient,
            userId: JwtSubject()
        )));

    /// <summary>
    /// Updates appointment. For patients and doctors
    /// </summary>
    [HttpPatch("user/current/{appointmentId:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointmentByIdAsync(
        UpdateAppointmentRequest request, Guid appointmentId)
        => CreateResponse(await Mediator.Send(new UpdateAppointmentCommand(
            request: request, appointmentId: appointmentId, userId: JwtSubject(), role: JwtRole()
        )));
}