using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.ChatMessages.GetChatMessagesByReceiverId;

public class GetChatMessagesByReceiverIdQuery : IRequest<HttpResult<PagedResponse<ChatMessageResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }

    public Guid ReceiverId { get; set; }

    public Guid SenderId { get; set; }
}