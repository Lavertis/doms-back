using DoctorsOffice.Application.CQRS.Commands.QuickButtons.CreateDoctorQuickButton;
using DoctorsOffice.Application.CQRS.Commands.QuickButtons.DeleteDoctorQuickButton;
using DoctorsOffice.Application.CQRS.Queries.QuickButtons.GetDoctorQuickButtons;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/quick-buttons")]
public class QuickButtonsController : BaseController
{
    public QuickButtonsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all quick buttons for authenticated doctor in groups.
    /// </summary>
    [HttpGet("doctor/current")]
    [Authorize]
    public async Task<ActionResult<DoctorQuickButtonsResponse>> GetDoctorQuickButtons()
        => CreateResponse(await Mediator.Send(new GetDoctorQuickButtonsQuery(JwtSubject())));

    /// <summary>
    /// Creates new quick button for authenticated doctor.
    /// </summary>
    [HttpPost("doctor/current")]
    [Authorize]
    public async Task<ActionResult<QuickButtonResponse>> CreateDoctorQuickButton(CreateQuickButtonRequest request)
        => CreateResponse(await Mediator.Send(new CreateDoctorQuickButtonCommand
        {
            DoctorId = JwtSubject(),
            Data = request
        }));

    [HttpDelete("doctor/current/{quickButtonId:guid}")]
    [Authorize]
    public async Task<ActionResult<Unit>> DeleteDoctorQuickButton(Guid quickButtonId)
        => CreateResponse(await Mediator.Send(new DeleteDoctorQuickButtonCommand
        {
            DoctorId = JwtSubject(),
            QuickButtonId = quickButtonId
        }));
}