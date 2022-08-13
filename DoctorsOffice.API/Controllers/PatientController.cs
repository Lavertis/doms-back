using DoctorsOffice.Application.CQRS.Commands.CreatePatient;
using DoctorsOffice.Application.CQRS.Commands.UpdatePatientById;
using DoctorsOffice.Application.CQRS.Queries.GetPatientById;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOfficeApi.CQRS.Commands.DeletePatientById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/patient")]
[Authorize(Roles = RoleTypes.Patient)]
public class PatientController : BaseController
{
    public PatientController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns authenticated patient.
    /// </summary>
    [HttpGet("auth")]
    public async Task<ActionResult<PatientResponse>> GetAuthenticatedPatientAsync()
        => Ok(await Mediator.Send(new GetPatientByIdQuery(patientId: JwtSubject())));

    /// <summary>
    /// Creates new patient.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<PatientResponse>> CreatePatientAsync(CreatePatientRequest request)
        => StatusCode(StatusCodes.Status201Created, await Mediator.Send(new CreatePatientCommand(request)));

    /// <summary>
    /// Updates account of the authenticated patient. Only for patients
    /// </summary>
    [HttpPatch("auth")]
    public async Task<ActionResult<PatientResponse>> UpdateAuthenticatedPatientAsync(
        UpdateAuthenticatedPatientRequest request)
        => Ok(await Mediator.Send(new UpdatePatientByIdCommand(request: request, patientId: JwtSubject())));

    /// <summary>
    /// Deletes account of the authenticated patient. Only for patients
    /// </summary>
    [HttpDelete("auth")]
    public async Task<ActionResult> DeleteAuthenticatedPatientAsync()
        => Ok(await Mediator.Send(new DeletePatientByIdCommand(patientId: JwtSubject())));
}