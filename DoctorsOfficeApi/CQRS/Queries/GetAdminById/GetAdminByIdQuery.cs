using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAdminById;

public class GetAdminByIdQuery : IRequest<AdminResponse>
{
    public readonly Guid AdminId;

    public GetAdminByIdQuery(Guid adminId)
    {
        AdminId = adminId;
    }
}