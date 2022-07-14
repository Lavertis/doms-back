using System.Security.Claims;
using DoctorsOfficeApi.CQRS.Commands.CreateDoctor;
using DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;
using DoctorsOfficeApi.CQRS.Commands.UpdateDoctorById;
using DoctorsOfficeApi.CQRS.Queries.GetAllDoctors;
using DoctorsOfficeApi.CQRS.Queries.GetDoctorById;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/doctor")]
[ApiExplorerSettings(GroupName = "Doctor")]
public class DoctorController : Controller
{
    private readonly IMediator _mediator;

    public DoctorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns authenticated doctor. Only for doctors
    /// </summary>
    [HttpGet("auth")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<DoctorResponse>> GetAuthenticatedDoctorAsync()
    {
        var authenticatedDoctorId = User.FindFirstValue(ClaimTypes.Sid)!;
        var getDoctorByIdQuery = new GetDoctorByIdQuery(authenticatedDoctorId);
        var authenticatedDoctorResponse = await _mediator.Send(getDoctorByIdQuery);
        return Ok(authenticatedDoctorResponse);
    }

    /// <summary>
    /// Returns all doctors. Only for admins
    /// </summary>
    [HttpGet("")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<IList<DoctorResponse>>> GetAllDoctorsAsync()
    {
        var getAllDoctorsQuery = new GetAllDoctorsQuery();
        var allDoctorResponses = await _mediator.Send(getAllDoctorsQuery);
        return Ok(allDoctorResponses);
    }

    /// <summary>
    /// Creates a new doctor. Only for admins
    /// </summary>
    [HttpPost("")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<DoctorResponse>> CreateDoctorAsync(CreateDoctorRequest request)
    {
        var createDoctorCommand = new CreateDoctorCommand(request);
        var createdDoctorResponse = await _mediator.Send(createDoctorCommand);
        return new ObjectResult(createdDoctorResponse) { StatusCode = StatusCodes.Status201Created };
    }

    /// <summary>
    /// Updates authenticated doctor. Only for doctors
    /// </summary>
    [HttpPatch("auth")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<DoctorResponse>> UpdateAuthenticatedDoctorAsync(
        UpdateAuthenticatedDoctorRequest request)
    {
        var authenticatedDoctorId = User.FindFirstValue(ClaimTypes.Sid)!;
        var updateDoctorByIdCommand = new UpdateDoctorByIdCommand(authenticatedDoctorId, request);
        var updatedDoctorResponse = await _mediator.Send(updateDoctorByIdCommand);
        return Ok(updatedDoctorResponse);
    }

    /// <summary>
    /// Updates doctor by id. Only for admins
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<DoctorResponse>> UpdateDoctorByIdAsync(string id, UpdateDoctorRequest request)
    {
        var updateDoctorByIdCommand = new UpdateDoctorByIdCommand(id, request);
        var updatedDoctorResponse = await _mediator.Send(updateDoctorByIdCommand);
        return Ok(updatedDoctorResponse);
    }

    /// <summary>
    /// Deletes doctor by id. Only for admins
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult> DeleteDoctorByIdAsync(string id)
    {
        var deleteDoctorByIdCommand = new DeleteDoctorByIdCommand(id);
        await _mediator.Send(deleteDoctorByIdCommand);
        return Ok();
    }
}