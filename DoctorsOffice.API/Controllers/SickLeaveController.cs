using DoctorsOffice.Application.CQRS.Commands.SickLeaves.CreateSickLeave;
using DoctorsOffice.Application.CQRS.Commands.SickLeaves.DeleteSickLeave;
using DoctorsOffice.Application.CQRS.Commands.SickLeaves.UpdateSickLeave;
using DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetAllSickLeaves;
using DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetAllSickLeavesByDoctorId;
using DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetAllSickLeavesByPatientId;
using DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetSickLeavesByAppointmentId;
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
[Route("api/sick-leaves")]
public class SickLeaveController : BaseController
{
    public SickLeaveController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all sick leaves. Only for doctors.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PagedResponse<SickLeaveResponse>>> GetAllAdminsAsync(
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetAllSickLeavesQuery {PaginationFilter = paginationFilter}));

    /// <summary>
    /// Returns all sick leaves for specified patient. Only for doctors.
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PagedResponse<SickLeaveResponse>>> GetSickLeavesByPatientIdAsync(
        Guid patientId, [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(
            await Mediator.Send(new GetSickLeavesByPatientIdQuery
            {
                PatientId = patientId,
                PaginationFilter = paginationFilter
            }));

    /// <summary>
    /// Returns all sick leaves for authenticated patient. Only for patients.
    /// </summary>
    [HttpGet("patient/current")]
    [Authorize(Roles = Roles.Patient)]
    public async Task<ActionResult<PagedResponse<SickLeaveResponse>>> GetSickLeavesForAuthenticatedPatientAsync(
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(
            await Mediator.Send(new GetSickLeavesByPatientIdQuery
            {
                PatientId = JwtSubject(),
                PaginationFilter = paginationFilter
            }));

    /// <summary>
    /// Returns all sick leaves for specified appointment. Only for doctors.
    /// </summary>
    [HttpGet("appointment/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PagedResponse<SickLeaveResponse>>>
        GetSickLeavesByAppointmentIdAsync(Guid appointmentId, [FromQuery] PaginationFilter filter)
        => CreateResponse(await Mediator.Send(new GetSickLeavesByAppointmentIdQuery
        {
            AppointmentId = appointmentId,
            DoctorId = JwtSubject(),
            PaginationFilter = filter
        }));

    /// <summary>
    /// Returns all sick leaves for specified appointment of authenticated patient.
    /// </summary>
    [HttpGet("patient/current/appointment/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Patient)]
    public async Task<ActionResult<PagedResponse<SickLeaveResponse>>>
        GetSickLeavesByAuthenticatedPatientAppointmentIdAsync(
            Guid appointmentId, [FromQuery] PaginationFilter filter)
        => CreateResponse(await Mediator.Send(new GetSickLeavesByAppointmentIdQuery
        {
            AppointmentId = appointmentId,
            PatientId = JwtSubject(),
            PaginationFilter = filter
        }));

    /// <summary>
    /// Returns all sick leaves for authenticated doctor. Only for doctors.
    /// </summary>
    [HttpGet("doctor/current")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PagedResponse<SickLeaveResponse>>> GetSickLeavesForAuthenticatedDoctorAsync(
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(
            await Mediator.Send(new GetSickLeavesByDoctorIdQuery()
            {
                DoctorId = JwtSubject(),
                PaginationFilter = paginationFilter
            }));

    /// <summary>
    /// Creates new sick leave. Only for doctors
    /// </summary>
    [HttpPost("doctor/current")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<SickLeaveResponse>> CreateSickLeaveAsync(CreateSickLeaveRequest request)
        => CreateResponse(await Mediator.Send(new CreateSickLeaveCommand(request, JwtSubject())));

    /// <summary>
    /// Updates sick leave by id. Only for doctors.
    /// </summary>
    [HttpPatch("{sickLeaveId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<SickLeaveResponse>> UpdateSickLeaveByIdAsync(
        UpdateSickLeaveRequest request, Guid sickLeaveId)
        => CreateResponse(
            await Mediator.Send(new UpdateSickLeaveCommand(request)
                {
                    SickLeaveId = sickLeaveId
                }
            ));

    /// <summary>
    /// Delete sick leave by id. Only for doctors.
    /// </summary>
    [HttpDelete("{sickLeaveId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<Unit>> DeleteSickLeaveByIdAsync(Guid sickLeaveId)
        => CreateResponse(await Mediator.Send(new DeleteSickLeaveCommand
        {
            SickLeaveId = sickLeaveId,
            DoctorId = JwtSubject()
        }));
}