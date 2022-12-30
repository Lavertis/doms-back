using DoctorsOffice.Application.CQRS.Queries.AppointmentStatuses.GetAllAppointmentStatuses;
using DoctorsOffice.Application.CQRS.Queries.AppointmentStatuses.GetAppointmentStatusById;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/appointment-statuses")]
public class AppointmentStatusController : BaseController
{
    public AppointmentStatusController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all appointment statuses.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AppointmentStatusResponse>>> GetAllAppointmentStatusesAsync()
        => CreateResponse(await Mediator.Send(new GetAllAppointmentStatusesQuery()));

    /// <summary>
    /// Appointment status by id. Only for doctors and patients
    /// </summary>
    [HttpGet("{appointmentStatusId:guid}")]
    [Authorize(Roles = $"{Roles.Doctor}, {Roles.Patient}")]
    public async Task<ActionResult<AppointmentStatusResponse>> GetAppointmentStatusByIdAsync(Guid appointmentStatusId)
        => CreateResponse(await Mediator.Send(new GetAppointmentStatusByIdQuery(appointmentStatusId)));
}