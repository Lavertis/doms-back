using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.ChatMessages.GetChatMessagesByReceiverId;

public class GetChatMessagesByReceiverHandler : IRequestHandler<GetChatMessagesByReceiverIdQuery,
    HttpResult<PagedResponse<ChatMessageResponse>>>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IMapper _mapper;

    public GetChatMessagesByReceiverHandler(IChatMessageRepository chatMessageRepository, IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<ChatMessageResponse>>> Handle(GetChatMessagesByReceiverIdQuery request,
        CancellationToken cancellationToken)
    {
        var chatMessages = _chatMessageRepository.GetAll()
            .Where(m => m.SenderId == request.SenderId || m.ReceiverId == request.SenderId)
            .Where(m => m.ReceiverId == request.ReceiverId || m.SenderId == request.ReceiverId)
            .OrderBy(m => m.CreatedAt);

        var chatMessagesResponsesQueryable = chatMessages
            .Select(m => _mapper.Map<ChatMessageResponse>(m));

        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(chatMessagesResponsesQueryable, request.PaginationFilter)
        );
    }
}