using System.Security.Claims;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AppointmentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/appointment")]
[ApiExplorerSettings(GroupName = "Appointment")]
public class AppointmentController : Controller
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Returns all appointments for authenticated user sorted by date. For doctors and patients
    /// </summary>
    [HttpGet("auth")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<IList<AppointmentResponse>>> GetAllAppointmentsForAuthenticatedUserAsync()
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.Sid)!;
        var authenticatedUserRole = User.FindFirstValue(ClaimTypes.Role)!;
        var appointments = authenticatedUserRole switch
        {
            RoleTypes.Doctor => await _appointmentService.GetAppointmentsByDoctorIdAsync(authenticatedUserId),
            RoleTypes.Patient => await _appointmentService.GetAppointmentsByPatientIdAsync(authenticatedUserId),
            _ => throw new ArgumentException("Invalid role")
        };

        var appointmentResponses = appointments
            .Select(appointment => new AppointmentResponse(appointment))
            .ToList();
        return Ok(appointmentResponses);
    }

    /// <summary>
    /// Returns appointment by id. User can only get owned appointment. For doctors and patients
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentByIdAsync(long id)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.Sid)!;
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment.Patient.Id != authenticatedUserId && appointment.Doctor.Id != authenticatedUserId)
            return StatusCode(403);

        var appointmentResponse = new AppointmentResponse(appointment);
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

        var appointments = await _appointmentService.GetFilteredAppointmentsAsync(
            dateStart,
            dateEnd,
            type,
            status,
            patientId,
            authenticatedUserId
        );

        var appointmentResponses = appointments
            .Select(appointment => new AppointmentResponse(appointment))
            .ToList();

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

        var appointments = await _appointmentService.GetFilteredAppointmentsAsync(
            dateStart,
            dateEnd,
            type,
            status,
            authenticatedUserId,
            null
        );

        var appointmentResponses = appointments
            .Select(appointment => new AppointmentResponse(appointment))
            .ToList();

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

        appointmentRequest.Status = AppointmentStatuses.Accepted;

        var appointment = await _appointmentService.CreateAppointmentAsync(appointmentRequest);

        var appointmentResponse = new AppointmentResponse(appointment);
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

        appointmentRequest.Status = AppointmentStatuses.Pending;

        var appointment = await _appointmentService.CreateAppointmentAsync(appointmentRequest);

        var appointmentResponse = new AppointmentResponse(appointment);
        return new ObjectResult(appointmentResponse) { StatusCode = StatusCodes.Status201Created };
    }

    /// <summary>
    /// Updates appointment. For patients and doctors
    /// </summary>
    [HttpPatch("{id:long}")]
    [Authorize(Roles = $"{RoleTypes.Doctor}, {RoleTypes.Patient}")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointmentByIdAsync(long id, UpdateAppointmentRequest appointmentRequest)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.Sid)!;
        var authenticatedUserRole = User.FindFirstValue(ClaimTypes.Role)!;

        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment.Patient.Id != authenticatedUserId && appointment.Doctor.Id != authenticatedUserId)
            return StatusCode(403);

        if (appointmentRequest.Status != null &&
            authenticatedUserRole == RoleTypes.Doctor &&
            !AppointmentStatuses.AllowedTransitions[appointment.Status.Name].Contains(appointmentRequest.Status!))
        {
            throw new BadRequestException($"Cant change status from {appointment.Status.Name} to {appointmentRequest.Status}");
        }

        var updatedAppointment = await _appointmentService.UpdateAppointmentByIdAsync(id, appointmentRequest);
        var appointmentResponse = new AppointmentResponse(updatedAppointment);
        return Ok(appointmentResponse);
    }
}