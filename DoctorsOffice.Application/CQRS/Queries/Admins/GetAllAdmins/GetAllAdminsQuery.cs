using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;

public class GetAllAdminsQuery : IRequest<HttpResult<IEnumerable<AdminResponse>>>
{
}