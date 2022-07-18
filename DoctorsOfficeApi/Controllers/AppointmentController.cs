using System.Security.Claims;
using DoctorsOfficeApi.CQRS.Commands.CreateAppointment;
using DoctorsOfficeApi.CQRS.Commands.UpdateAppointment;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByDoctorId;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByPatientId;
using DoctorsOfficeApi.CQRS.Queries.GetFilteredAppointments;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/appointment")]
[ApiExplorerSettings(GroupName = "Appointment")]
public class AppointmentController : Controller
{
    private readonly IMediator _mediator;

    public AppointmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all appointments for authenticated user sorted by date. For doctors and patients
    /// </summary>
    [HttpGet("auth")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<IList<AppointmentResponse>>> GetAllAppointmentsForAuthenticatedUserAsync()
    {
        var authenticatedUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!);
        var authenticatedUserRole = User.FindFirstValue(ClaimTypes.Role)!;

        var appointmentResponses = authenticatedUserRole switch
        {
            RoleTypes.Doctor => await _mediator.Send(new GetAppointmentsByDoctorIdQuery(authenticatedUserId)),
            RoleTypes.Patient => await _mediator.Send(new GetAppointmentsByPatientIdQuery(authenticatedUserId)),
            _ => throw new ArgumentException("Invalid role")
        };

        return Ok(appointmentResponses);
    }

    /// <summary>
    /// Returns appointment by id. User can only get owned appointment. For doctors and patients
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentByIdAsync(Guid id)
    {
        var authenticatedUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!);
        var query = new GetAppointmentByIdQuery(id);
        var appointmentResponse = await _mediator.Send(query);

        if (appointmentResponse.PatientId != authenticatedUserId && appointmentResponse.DoctorId != authenticatedUserId)
            return StatusCode(403);

        return Ok(appointmentResponse);
    }

    /// <summary>
    /// Returns all appointments matching search criteria sorted by date. Only for doctors
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<IList<AppointmentResponse>>> GetAppointmentsFilteredAsync(
        DateTime? dateStart,
        DateTime? dateEnd,
        string? patientId,
        string? type,
        string? status)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.Sid)!;

        var query = new GetFilteredAppointmentsQuery(dateStart, dateEnd, type, status, patientId, authenticatedUserId);
        var appointmentResponses = await _mediator.Send(query);

        return Ok(appointmentResponses);
    }

    /// <summary>
    /// Returns appointments for authenticated user matching search criteria sorted by date. Only for patients
    /// </summary>
    [HttpGet("auth/search")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<IList<AppointmentResponse>>> GetAppointmentsForAuthenticatedUserFiltered(
        DateTime? dateStart,
        DateTime? dateEnd,
        string? type,
        string? status)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.Sid)!;

        var query = new GetFilteredAppointmentsQuery(
            dateStart,
            dateEnd,
            type,
            status,
            authenticatedUserId,
            null
        );
        var appointmentResponses = await _mediator.Send(query);

        return Ok(appointmentResponses);
    }

    /// <summary>
    /// Creates new appointment. Only for doctors
    /// </summary>
    [HttpPost]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest appointmentRequest)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.Sid)!;
        if (appointmentRequest.DoctorId != authenticatedUserId)
            throw new BadRequestException("Cannot create appointment for another doctor");

        var command = new CreateAppointmentCommand(appointmentRequest)
        {
            Status = AppointmentStatuses.Accepted
        };
        var appointmentResponse = await _mediator.Send(command);

        return new ObjectResult(appointmentResponse) { StatusCode = StatusCodes.Status201Created };
    }

    /// <summary>
    /// Creates appointment request. Only for patients
    /// </summary>
    [HttpPost("request")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointmentRequestAsync(CreateAppointmentRequest appointmentRequest)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.Sid)!;
        if (appointmentRequest.PatientId != authenticatedUserId)
            throw new BadRequestException("Cannot create appointment request for another user");

        var command = new CreateAppointmentCommand(appointmentRequest)
        {
            Status = AppointmentStatuses.Pending
        };
        var appointmentResponse = await _mediator.Send(command);

        return new ObjectResult(appointmentResponse) { StatusCode = StatusCodes.Status201Created };
    }

    /// <summary>
    /// Updates appointment. For patients and doctors
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointmentByIdAsync(Guid id, UpdateAppointmentRequest appointmentRequest)
    {
        var authenticatedUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!);
        var authenticatedUserRole = User.FindFirstValue(ClaimTypes.Role)!;

        var getAppointmentByIdQuery = new GetAppointmentByIdQuery(id);
        var oldAppointmentResponse = await _mediator.Send(getAppointmentByIdQuery);

        if (oldAppointmentResponse.PatientId != authenticatedUserId && oldAppointmentResponse.DoctorId != authenticatedUserId)
            return StatusCode(403);

        if (!string.IsNullOrEmpty(appointmentRequest.Status) &&
            authenticatedUserRole == RoleTypes.Doctor &&
            !AppointmentStatuses.AllowedTransitions[oldAppointmentResponse.Status].Contains(appointmentRequest.Status!))
        {
            throw new BadRequestException($"Cant change status from {oldAppointmentResponse.Status} to {appointmentRequest.Status}");
        }

        var command = new UpdateAppointmentCommand(id, appointmentRequest);
        var updatedAppointment = await _mediator.Send(command);

        return Ok(updatedAppointment);
    }
}