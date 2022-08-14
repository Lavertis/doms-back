using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Admins.GetAdminById;

public class GetAdminByIdQuery : IRequest<HttpResult<AdminResponse>>
{
    public readonly Guid AdminId;

    public GetAdminByIdQuery(Guid adminId)
    {
        AdminId = adminId;
    }
}