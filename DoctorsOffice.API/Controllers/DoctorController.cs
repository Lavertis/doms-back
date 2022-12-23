using DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;
using DoctorsOffice.Application.CQRS.Commands.Doctors.DeleteDoctorById;
using DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;
using DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorById;
using DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorsFiltered;
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
[Route("api/doctors")]
public class DoctorController : BaseController
{
    public DoctorController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns filtered doctors matching search criteria.
    /// </summary>
    [HttpGet("")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<DoctorResponse>>> GetDoctorsFilteredAsync(
        [FromQuery] DoctorQueryParams queryParams, [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetDoctorsFilteredQuery
        {
            PaginationFilter = paginationFilter,
            QueryParams = queryParams
        }));

    /// <summary>
    /// Returns authenticated doctor. Only for doctors
    /// </summary>
    [HttpGet("current")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<DoctorResponse>> GetAuthenticatedDoctorAsync()
        => CreateResponse(await Mediator.Send(new GetDoctorByIdQuery(JwtSubject())));

    /// <summary>
    /// Returns doctor by id. Only for doctors
    /// </summary>
    [HttpGet("{doctorId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DoctorResponse>> GetDoctorByIdAsync(Guid doctorId)
        => CreateResponse(await Mediator.Send(new GetDoctorByIdQuery(doctorId)));

    /// <summary>
    /// Creates a new doctor. Only for admins
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CreateDoctorResponse>> CreateDoctorAsync(CreateDoctorRequest request)
        => CreateResponse(await Mediator.Send(new CreateDoctorCommand(request)));

    /// <summary>
    /// Updates authenticated doctor. Only for doctors
    /// </summary>
    [HttpPatch("current")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<DoctorResponse>> UpdateAuthenticatedDoctorAsync(
        UpdateAuthenticatedDoctorRequest request)
        => CreateResponse(await Mediator.Send(new UpdateDoctorByIdCommand(request, JwtSubject())));

    /// <summary>
    /// Updates doctor by id. Only for admins
    /// </summary>
    [HttpPatch("{doctorId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DoctorResponse>> UpdateDoctorByIdAsync(Guid doctorId, UpdateDoctorRequest request)
        => CreateResponse(await Mediator.Send(new UpdateDoctorByIdCommand(request, doctorId)));

    /// <summary>
    /// Deletes doctor by id. Only for admins
    /// </summary>
    [HttpDelete("{doctorId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<Unit>> DeleteDoctorByIdAsync(Guid doctorId)
        => CreateResponse(await Mediator.Send(new DeleteDoctorByIdCommand(doctorId)));
}