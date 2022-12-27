using DoctorsOffice.Application.CQRS.Queries.Statistics.GetAdminStatisticsByDate;
using DoctorsOffice.Application.CQRS.Queries.Statistics.GetDoctorStatisticsByDate;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : BaseController
{
    public StatisticsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns statistic for current logged in doctor sorted by date. Only for doctors
    /// </summary>
    [HttpGet("doctor/current")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<DoctorStatisticsResponse>> GetStatisticsAsync(
        DateTime? dateStart,
        DateTime? dateEnd
    )
        => CreateResponse(await Mediator.Send(new GetDoctorStatisitcsByDateQuery
        {
            DateStart = dateStart,
            DateEnd = dateEnd,
            DoctorId = JwtSubject()
        }));

    /// <summary>
    /// Returns statistics for doctor. Only for admin
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AdminStatisticsResponse>> GetStatisticsAsync(
        DateTime? dateStart,
        DateTime? dateEnd,
        Guid? doctorId
    )
        => CreateResponse(await Mediator.Send(new GetAdminStatisitcsByDateQuery
        {
            DateStart = dateStart,
            DateEnd = dateEnd,
            DoctorId = doctorId
        }));
}