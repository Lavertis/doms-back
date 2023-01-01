using DoctorsOffice.Application.CQRS.Queries.ChatMessages.GetChatMessagesByReceiverId;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/chat-messages")]
public class ChatController : BaseController
{
    public ChatController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns chat messages all between other user for authenticated user. Ordered by date
    /// </summary>
    [HttpGet("user/current/{receiverId:guid}")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<ChatMessageResponse>>>
        GetChatMessagesForAuthenticatedUserAsync(Guid receiverId, [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(
            await Mediator.Send(new GetChatMessagesByReceiverIdQuery()
                {
                    SenderId = JwtSubject(),
                    ReceiverId = receiverId,
                    PaginationFilter = paginationFilter
                }
            ));
}