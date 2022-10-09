using DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;
using DoctorsOffice.Application.CQRS.Commands.Prescriptions.DeletePrescription;
using DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByAppointmentId;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByDoctorId;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByPatientId;
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
[Route("api/prescriptions")]
public class PrescriptionController : BaseController
{
    public PrescriptionController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all prescriptions for specified appointment. Only for doctors.
    /// </summary>
    [HttpGet("appointment/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PagedResponse<PrescriptionResponse>>>
        GetPrescriptionsByAppointmentIdAsync(Guid appointmentId, [FromQuery] PaginationFilter filter)
        => CreateResponse(await Mediator.Send(new GetPrescriptionsByAppointmentIdQuery
        {
            AppointmentId = appointmentId,
            DoctorId = JwtSubject(),
            PaginationFilter = filter
        }));

    /// <summary>
    /// Returns all prescriptions for specified patient. Only for doctors.
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PagedResponse<PrescriptionResponse>>> GetPrescriptionsByPatientIdAsync(
        Guid patientId, [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(
            await Mediator.Send(new GetPrescriptionsByPatientIdQuery
            {
                PatientId = patientId,
                PaginationFilter = paginationFilter
            }));

    /// <summary>
    /// Returns all prescriptions for authenticated patient. Only for patients.
    /// </summary>
    [HttpGet("patient/current")]
    [Authorize(Roles = Roles.Patient)]
    public async Task<ActionResult<PagedResponse<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedPatientAsync(
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(
            await Mediator.Send(new GetPrescriptionsByPatientIdQuery
            {
                PatientId = JwtSubject(),
                PaginationFilter = paginationFilter
            }));

    /// <summary>
    /// Returns all prescriptions for authenticated doctor. Only for doctors.
    /// </summary>
    [HttpGet("doctor/current")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PagedResponse<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedDoctorAsync(
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetPrescriptionsByDoctorIdQuery
        {
            DoctorId = JwtSubject(),
            PaginationFilter = paginationFilter
        }));

    /// <summary>
    /// Creates new prescription. Only for doctors.
    /// </summary>
    [HttpPost("doctor/current")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> CreatePrescriptionAsync(CreatePrescriptionRequest request)
        => CreateResponse(await Mediator.Send(new CreatePrescriptionCommand(request, JwtSubject())));

    /// <summary>
    /// Updates prescription by id. Only for doctors.
    /// </summary>
    [HttpPatch("{prescriptionId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> UpdatePrescriptionByIdAsync(
        UpdatePrescriptionRequest request, Guid prescriptionId)
        => CreateResponse(
            await Mediator.Send(new UpdatePrescriptionCommand(request, prescriptionId))
        );

    /// <summary>
    /// Delete prescription by id. Only for doctors.
    /// </summary>
    [HttpDelete("{prescriptionId:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<Unit>> DeletePrescriptionByIdAsync(Guid prescriptionId)
        => CreateResponse(await Mediator.Send(new DeletePrescriptionCommand
        {
            PrescriptionId = prescriptionId,
            DoctorId = JwtSubject()
        }));
}