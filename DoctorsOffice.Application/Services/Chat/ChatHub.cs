using DoctorsOffice.Application.CQRS.Commands.ChatMessages.CreateChatMessage;
using DoctorsOffice.Domain.DTO.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DoctorsOffice.Application.Services.Chat;

public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator)
    {
        this._mediator = mediator;
    }

    [Authorize]
    public async Task SendMessage(string receiverId, string message)
    {
        if (Context.UserIdentifier == null)
        {
            throw new UnauthorizedAccessException("Connection only for authorized users.");
        }

        await _mediator.Send(new CreateChatMessageCommand(new CreateChatMessageRequest()
            {
                Message = message,
                ReceiverId = new Guid(receiverId),
                SenderId = new Guid(Context.UserIdentifier)
            }
        ));

        await Clients.User(receiverId).SendAsync("ReceiveMessage", Context.UserIdentifier, message);
    }
}