using System.Security.Claims;
using DoctorsOfficeApi.CQRS.Commands.CreatePatient;
using DoctorsOfficeApi.CQRS.Commands.DeletePatientById;
using DoctorsOfficeApi.CQRS.Commands.UpdatePatientById;
using DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/patient")]
[Authorize(Roles = RoleTypes.Patient)]
[ApiExplorerSettings(GroupName = "Patient")]
public class PatientController : Controller
{
    private readonly IMediator _mediator;

    public PatientController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns authenticated patient.
    /// </summary>
    [HttpGet("auth")]
    public async Task<ActionResult<PatientResponse>> GetAuthenticatedPatientAsync()
    {
        var authenticatedPatientId = Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!);
        var query = new GetPatientByIdQuery(authenticatedPatientId);
        var patient = await _mediator.Send(query);
        return Ok(patient);
    }

    /// <summary>
    /// Creates new patient.
    /// </summary>
    [HttpPost("")]
    [AllowAnonymous]
    public async Task<ActionResult<PatientResponse>> CreatePatientAsync(CreatePatientRequest request)
    {
        var command = new CreatePatientCommand(request);
        var patientResponse = await _mediator.Send(command);
        return new ObjectResult(patientResponse) { StatusCode = StatusCodes.Status201Created };
    }

    /// <summary>
    /// Updates account of the authenticated patient. Only for patients
    /// </summary>
    [HttpPatch("auth")]
    public async Task<ActionResult<PatientResponse>> UpdateAuthenticatedPatientAsync(UpdateAuthenticatedPatientRequest request)
    {
        var authenticatedPatientId = Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!);
        var command = new UpdatePatientByIdCommand(authenticatedPatientId, request);
        var patientResponse = await _mediator.Send(command);
        return Ok(patientResponse);
    }

    /// <summary>
    /// Deletes account of the authenticated patient. Only for patients
    /// </summary>
    [HttpDelete("auth")]
    public async Task<ActionResult> DeleteAuthenticatedPatientAsync()
    {
        var authenticatedPatientId = Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!);
        var command = new DeletePatientByIdCommand(authenticatedPatientId);
        await _mediator.Send(command);
        return Ok();
    }
}