using DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;
using DoctorsOffice.Application.CQRS.Commands.Patients.DeletePatientById;
using DoctorsOffice.Application.CQRS.Commands.Patients.UpdatePatientById;
using DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientById;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize(Roles = RoleTypes.Patient)]
public class PatientController : BaseController
{
    public PatientController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns authenticated patient.
    /// </summary>
    [HttpGet("current")]
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
    [HttpPatch("current")]
    public async Task<ActionResult<PatientResponse>> UpdateAuthenticatedPatientAsync(
        UpdateAuthenticatedPatientRequest request)
        => Ok(await Mediator.Send(new UpdatePatientByIdCommand(request: request, patientId: JwtSubject())));

    /// <summary>
    /// Deletes account of the authenticated patient. Only for patients
    /// </summary>
    [HttpDelete("current")]
    public async Task<ActionResult> DeleteAuthenticatedPatientAsync()
        => Ok(await Mediator.Send(new DeletePatientByIdCommand(patientId: JwtSubject())));
}