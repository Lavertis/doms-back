using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAdminById;

public class GetAdminByIdQuery : IRequest<AdminResponse>
{
    public Guid Id { get; set; }

    public GetAdminByIdQuery(Guid id)
    {
        Id = id;
    }
}