using DoctorsOffice.Application.CQRS.Queries.AppointmentTypes.GetAllAppointmentTypes;
using DoctorsOffice.Application.CQRS.Queries.AppointmentTypes.GetAppointmentTypeById;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/appointment-types")]
public class AppointmentTypeController : BaseController
{
    public AppointmentTypeController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all appointment types.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AppointmentTypeResponse>>> GetAllAppointmentTypesAsync()
        => CreateResponse(await Mediator.Send(new GetAllAppointmentTypesQuery()));

    /// <summary>
    /// Appointment type by id. Only for doctors and patients
    /// </summary>
    [HttpGet("{appointmentTypeId:guid}")]
    [Authorize(Roles = $"{Roles.Doctor}, {Roles.Patient}")]
    public async Task<ActionResult<AppointmentTypeResponse>> GetAppointmentTypeByIdAsync(Guid appointmentTypeId)
        => CreateResponse(await Mediator.Send(new GetAppointmentTypeByIdQuery(appointmentTypeId)));
}