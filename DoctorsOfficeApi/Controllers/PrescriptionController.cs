using System.Security.Claims;
using DoctorsOfficeApi.CQRS.Commands.CreatePrescription;
using DoctorsOfficeApi.CQRS.Commands.UpdatePrescription;
using DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByDoctorId;
using DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByPatientId;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/prescription")]
[ApiExplorerSettings(GroupName = "Prescription")]
public class PrescriptionController : Controller
{
    private readonly IMediator _mediator;

    public PrescriptionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all prescriptions for specified patient. Only for doctors.
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<IList<PrescriptionResponse>>> GetPrescriptionsByPatientIdAsync(Guid patientId)
    {
        var query = new GetPrescriptionsByPatientIdQuery(patientId);
        var prescriptionResponses = await _mediator.Send(query);
        return Ok(prescriptionResponses);
    }

    /// <summary>
    /// Returns all prescriptions for authenticated patient. Only for patients.
    /// </summary>
    [HttpGet("patient/auth")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<IList<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedPatientAsync()
    {
        var authenticatedUserId = Guid.Parse(User.FindFirst(ClaimTypes.Sid)!.Value);
        var query = new GetPrescriptionsByPatientIdQuery(authenticatedUserId);
        var prescriptionResponses = await _mediator.Send(query);
        return Ok(prescriptionResponses);
    }

    /// <summary>
    /// Returns all prescriptions for authenticated doctor. Only for doctors.
    /// </summary>
    [HttpGet("doctor/auth")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<IList<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedDoctorAsync()
    {
        var authenticatedUserId = Guid.Parse(User.FindFirst(ClaimTypes.Sid)!.Value);
        var query = new GetPrescriptionsByDoctorIdQuery(authenticatedUserId);
        var prescriptionResponses = await _mediator.Send(query);
        return Ok(prescriptionResponses);
    }

    /// <summary>
    /// Creates new prescription. Only for doctors.
    /// </summary>
    [HttpPost("")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> CreatePrescriptionAsync(CreatePrescriptionRequest request)
    {
        var authenticatedUserId = Guid.Parse(User.FindFirst(ClaimTypes.Sid)!.Value);
        var command = new CreatePrescriptionCommand
        {
            Title = request.Title,
            Description = request.Description,
            PatientId = Guid.Parse(request.PatientId),
            DoctorId = authenticatedUserId,
            DrugsIds = request.DrugsIds,
        };
        var prescriptionResponse = await _mediator.Send(command);
        return new ObjectResult(prescriptionResponse) { StatusCode = StatusCodes.Status201Created };
    }

    /// <summary>
    /// Updates prescription by id prescription. Only for doctors.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> UpdatePrescriptionByIdAsync(Guid id, UpdatePrescriptionRequest request)
    {
        var command = new UpdatePrescriptionCommand(id, request);
        var prescriptionResponse = await _mediator.Send(command);
        return Ok(prescriptionResponse);
    }
}