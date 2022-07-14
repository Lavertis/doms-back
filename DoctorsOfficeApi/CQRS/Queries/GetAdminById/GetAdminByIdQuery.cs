using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAdminById;

public class GetAdminByIdQuery : IRequest<AdminResponse>
{
    public string Id { get; set; }

    public GetAdminByIdQuery(string id)
    {
        Id = id;
    }
}