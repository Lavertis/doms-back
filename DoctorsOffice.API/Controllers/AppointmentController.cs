using DoctorsOffice.Application.CQRS.Commands.CreateAppointment;
using DoctorsOffice.Application.CQRS.Commands.UpdateAppointment;
using DoctorsOffice.Application.CQRS.Queries.GetAppointmentById;
using DoctorsOffice.Application.CQRS.Queries.GetAppointmentsByUser;
using DoctorsOffice.Application.CQRS.Queries.GetFilteredAppointments;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/appointment")]
public class AppointmentController : BaseController
{
    public AppointmentController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all appointments for authenticated user sorted by date. For doctors and patients.
    /// </summary>
    [HttpGet("auth")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<IList<AppointmentResponse>>> GetAllAppointmentsForAuthenticatedUserAsync()
        => Ok(await Mediator.Send(new GetAppointmentsByUserQuery(userId: JwtSubject(), role: JwtRole())));

    /// <summary>
    /// Returns appointment by id. User can only get owned appointment. For doctors and patients
    /// </summary>
    [HttpGet("{appointmentId:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentByIdAsync(Guid appointmentId)
        => Ok(await Mediator.Send(new GetAppointmentByIdQuery(
            appointmentId: appointmentId, userId: JwtSubject(), roleName: JwtRole()
        )));

    /// <summary>
    /// Returns all appointments matching search criteria sorted by date. Only for doctors
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<IList<AppointmentResponse>>> GetAppointmentsFilteredAsync(
        DateTime? dateStart, DateTime? dateEnd, Guid? patientId, string? type, string? status)
        => Ok(await Mediator.Send(new GetFilteredAppointmentsQuery(
            new GetAppointmentsFilteredRequest
            {
                DateStart = dateStart,
                DateEnd = dateEnd,
                Type = type,
                Status = status,
                PatientId = patientId,
                DoctorId = JwtSubject()
            })));

    /// <summary>
    /// Returns appointments for authenticated user matching search criteria sorted by date. Only for patients
    /// </summary>
    [HttpGet("auth/search")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<IList<AppointmentResponse>>> GetAppointmentsForAuthenticatedUserFiltered(
        DateTime? dateStart, DateTime? dateEnd, string? type, string? status)
        => Ok(await Mediator.Send(new GetFilteredAppointmentsQuery(
            new GetAppointmentsFilteredRequest
            {
                DateStart = dateStart,
                DateEnd = dateEnd,
                Type = type,
                Status = status,
                PatientId = JwtSubject()
            })));

    /// <summary>
    /// Creates new appointment. Only for doctors
    /// </summary>
    [HttpPost]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request)
        => StatusCode(StatusCodes.Status201Created, await Mediator.Send(new CreateAppointmentCommand(
            request: request,
            status: AppointmentStatuses.Accepted,
            role: RoleTypes.Doctor,
            userId: JwtSubject()
        )));

    /// <summary>
    /// Creates appointment request. Only for patients
    /// </summary>
    [HttpPost("request")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentRequestAsync(CreateAppointmentRequest request)
        => StatusCode(StatusCodes.Status201Created, await Mediator.Send(new CreateAppointmentCommand(
            request: request,
            status: AppointmentStatuses.Pending,
            role: RoleTypes.Patient,
            userId: JwtSubject()
        )));

    /// <summary>
    /// Updates appointment. For patients and doctors
    /// </summary>
    [HttpPatch("{appointmentId:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointmentByIdAsync(
        UpdateAppointmentRequest request, Guid appointmentId)
        => Ok(await Mediator.Send(new UpdateAppointmentCommand(
            request: request, appointmentId: appointmentId, userId: JwtSubject(), role: JwtRole()
        )));
}