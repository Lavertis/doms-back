using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.ChatMessages.CreateChatMessage;

public class CreateChatMessageCommand : IRequest<ChatMessageResponse>
{
    public readonly string Message;

    public readonly Guid SenderId;

    public readonly Guid ReceiverId;

    public CreateChatMessageCommand(CreateChatMessageRequest request)
    {
        Message = request.Message;
        SenderId = request.SenderId;
        ReceiverId = request.ReceiverId;
    }
}