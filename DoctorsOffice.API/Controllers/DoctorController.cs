using DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;
using DoctorsOffice.Application.CQRS.Commands.Doctors.DeleteDoctorById;
using DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;
using DoctorsOffice.Application.CQRS.Queries.Doctors.GetAllDoctors;
using DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorById;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
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
    /// Returns authenticated doctor. Only for doctors
    /// </summary>
    [HttpGet("current")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<DoctorResponse>> GetAuthenticatedDoctorAsync()
        => CreateResponse(await Mediator.Send(new GetDoctorByIdQuery(JwtSubject())));

    /// <summary>
    /// Returns all doctors. Only for admins
    /// </summary>
    [HttpGet]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<IEnumerable<DoctorResponse>>> GetAllDoctorsAsync()
        => CreateResponse(await Mediator.Send(new GetAllDoctorsQuery()));

    /// <summary>
    /// Creates a new doctor. Only for admins
    /// </summary>
    [HttpPost]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<DoctorResponse>> CreateDoctorAsync(CreateDoctorRequest request)
        => CreateResponse(await Mediator.Send(new CreateDoctorCommand(request)));

    /// <summary>
    /// Updates authenticated doctor. Only for doctors
    /// </summary>
    [HttpPatch("current")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<DoctorResponse>> UpdateAuthenticatedDoctorAsync(
        UpdateAuthenticatedDoctorRequest request)
        => CreateResponse(await Mediator.Send(new UpdateDoctorByIdCommand(request, JwtSubject())));

    /// <summary>
    /// Updates doctor by id. Only for admins
    /// </summary>
    [HttpPatch("{doctorId:guid}")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<DoctorResponse>> UpdateDoctorByIdAsync(Guid doctorId, UpdateDoctorRequest request)
        => CreateResponse(await Mediator.Send(new UpdateDoctorByIdCommand(request, doctorId)));

    /// <summary>
    /// Deletes doctor by id. Only for admins
    /// </summary>
    [HttpDelete("{doctorId:guid}")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<Unit>> DeleteDoctorByIdAsync(Guid doctorId)
        => CreateResponse(await Mediator.Send(new DeleteDoctorByIdCommand(doctorId)));
}