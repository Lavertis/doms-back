﻿using DoctorsOfficeApi.CQRS.Commands.CreateDoctor;
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
public class DoctorController : BaseController
{
    public DoctorController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns authenticated doctor. Only for doctors
    /// </summary>
    [HttpGet("auth")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<DoctorResponse>> GetAuthenticatedDoctorAsync()
        => Ok(await Mediator.Send(new GetDoctorByIdQuery(doctorId: JwtSubject())));

    /// <summary>
    /// Returns all doctors. Only for admins
    /// </summary>
    [HttpGet("")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<IList<DoctorResponse>>> GetAllDoctorsAsync()
        => Ok(await Mediator.Send(new GetAllDoctorsQuery()));

    /// <summary>
    /// Creates a new doctor. Only for admins
    /// </summary>
    [HttpPost("")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<DoctorResponse>> CreateDoctorAsync(CreateDoctorRequest request)
        => StatusCode(StatusCodes.Status201Created, Ok(await Mediator.Send(new CreateDoctorCommand(request))));

    /// <summary>
    /// Updates authenticated doctor. Only for doctors
    /// </summary>
    [HttpPatch("auth")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<DoctorResponse>> UpdateAuthenticatedDoctorAsync(
        UpdateAuthenticatedDoctorRequest request)
        => Ok(await Mediator.Send(new UpdateDoctorByIdCommand(request: request, doctorId: JwtSubject())));

    /// <summary>
    /// Updates doctor by id. Only for admins
    /// </summary>
    [HttpPatch("{doctorId:guid}")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult<DoctorResponse>> UpdateDoctorByIdAsync(Guid doctorId, UpdateDoctorRequest request)
        => Ok(await Mediator.Send(new UpdateDoctorByIdCommand(request: request, doctorId: doctorId)));

    /// <summary>
    /// Deletes doctor by id. Only for admins
    /// </summary>
    [HttpDelete("{doctorId:guid}")]
    [Authorize(Roles = RoleTypes.Admin)]
    public async Task<ActionResult> DeleteDoctorByIdAsync(Guid doctorId)
        => Ok(await Mediator.Send(new DeleteDoctorByIdCommand(doctorId: doctorId)));
}