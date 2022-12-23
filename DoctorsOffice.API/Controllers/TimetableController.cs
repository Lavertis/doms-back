using DoctorsOffice.Application.CQRS.Commands.Timetables.CreateTimetables;
using DoctorsOffice.Application.CQRS.Commands.Timetables.DeleteTimetables;
using DoctorsOffice.Application.CQRS.Commands.Timetables.UpdateTimetables;
using DoctorsOffice.Application.CQRS.Queries.Timetables.GetTimetablesByDoctorId;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/timetables")]
public class TimetableController : BaseController
{
    public TimetableController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all timetables by doctor id. For doctors and patients.
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}")]
    [Authorize(Roles = $"{Roles.Doctor}, {Roles.Patient}")]
    public async Task<ActionResult<IEnumerable<TimetableResponse>>> GetAllTimetablesByDoctorIdAsync(
        Guid doctorId, DateTime? startDateTime, DateTime? endDateTime
    ) => CreateResponse(await Mediator.Send(new GetTimetablesByDoctorIdQuery
    {
        DoctorId = doctorId,
        StartDateTime = startDateTime,
        EndDateTime = endDateTime
    }));

    /// <summary>
    /// Creates list of timetables for authenticated doctor. Only for doctors.
    /// </summary>
    [HttpPost("doctor/current/batch")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<IEnumerable<TimetableResponse>>> CreateTimetablesForAuthenticatedDoctorAsync(
        CreateTimetableRequestList requests
    ) => CreateResponse(await Mediator.Send(new CreateTimetablesCommand
    {
        Data = requests,
        DoctorId = JwtSubject()
    }));

    /// <summary>
    /// Updates list of timetables for authenticated doctor. Only for doctors.
    /// </summary>
    [HttpPatch("batch")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<IEnumerable<TimetableResponse>>> UpdateTimetablesAsync(
        UpdateTimetableBatchRequestList requests
    ) => CreateResponse(await Mediator.Send(new UpdateTimetablesCommand(data: requests)));


    /// <summary>
    /// Deletes list of timetables by id. Only for doctors.
    /// </summary>
    [HttpDelete("batch")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<Unit>> DeleteTimetablesByIdAsync(IEnumerable<Guid> ids)
        => CreateResponse(await Mediator.Send(new DeleteTimetablesCommand(ids: ids)));
}