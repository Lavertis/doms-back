using DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;
using DoctorsOffice.Application.CQRS.Commands.Patients.DeletePatientById;
using DoctorsOffice.Application.CQRS.Commands.Patients.UpdatePatientById;
using DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientById;
using DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientsFiltered;
using DoctorsOffice.Domain.DTO.QueryParams;
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
[Route("api/patients")]
public class PatientController : BaseController
{
    public PatientController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns filtered patients matching search criteria. For admins and doctors.
    /// </summary>
    [HttpGet("")]
    [Authorize(Roles = $"{Roles.Admin}, {Roles.Doctor}")]
    public async Task<ActionResult<PagedResponse<PatientResponse>>> GetDoctorsFilteredAsync(
        [FromQuery] PatientQueryParams queryParams, [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetPatientsFilteredQuery
        {
            PaginationFilter = paginationFilter,
            QueryParams = queryParams
        }));

    /// <summary>
    /// Get patient by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin}, {Roles.Doctor}")]
    public async Task<ActionResult<PatientResponse>> GetPatientByIdAsync(Guid id)
        => CreateResponse(await Mediator.Send(new GetPatientByIdQuery(id)));

    /// <summary>
    /// Returns authenticated patient.
    /// </summary>
    [HttpGet("current")]
    [Authorize(Roles = Roles.Patient)]
    public async Task<ActionResult<PatientResponse>> GetAuthenticatedPatientAsync()
        => CreateResponse(await Mediator.Send(new GetPatientByIdQuery(JwtSubject())));

    /// <summary>
    /// Creates new patient.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreatePatientResponse>> CreatePatientAsync(CreatePatientRequest request)
        => CreateResponse(await Mediator.Send(new CreatePatientCommand(request)));

    /// <summary>
    /// Updates account of the authenticated patient. Only for patients
    /// </summary>
    [HttpPatch("current")]
    [Authorize(Roles = Roles.Patient)]
    public async Task<ActionResult<PatientResponse>> UpdateAuthenticatedPatientAsync(
        UpdateAuthenticatedPatientRequest request)
        => CreateResponse(await Mediator.Send(new UpdatePatientByIdCommand(request, JwtSubject())));

    /// <summary>
    /// Deletes account of the authenticated patient. Only for patients
    /// </summary>
    [HttpDelete("current")]
    [Authorize(Roles = Roles.Patient)]
    public async Task<ActionResult<Unit>> DeleteAuthenticatedPatientAsync()
        => CreateResponse(await Mediator.Send(new DeletePatientByIdCommand(JwtSubject())));

    /// <summary>
    /// Deletes patient by id. Only for admins
    /// </summary>
    [HttpDelete("{patientId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<Unit>> DeletePatientByIdAsync(Guid patientId)
        => CreateResponse(await Mediator.Send(new DeletePatientByIdCommand(patientId)));
}