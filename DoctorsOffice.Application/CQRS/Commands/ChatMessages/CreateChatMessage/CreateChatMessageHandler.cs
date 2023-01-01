using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.ChatMessages.CreateChatMessage;

public class CreateChatMessageHandler : IRequestHandler<CreateChatMessageCommand, ChatMessageResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IMapper _mapper;

    public CreateChatMessageHandler(IChatMessageRepository chatMessageRepository, IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _mapper = mapper;
    }

    public async Task<ChatMessageResponse> Handle(CreateChatMessageCommand request, CancellationToken cancellationToken)
    {
        var chatMessage = new ChatMessage()
        {
            Message = request.Message,
            ReceiverId = request.ReceiverId,
            SenderId = request.SenderId
        };

        var chatMessageResponse = _mapper.Map<ChatMessageResponse>(
            await _chatMessageRepository.CreateAsync(chatMessage)
        );

        return chatMessageResponse;
    }
}