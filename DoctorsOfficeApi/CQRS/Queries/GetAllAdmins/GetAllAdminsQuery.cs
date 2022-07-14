using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllAdmins;

public class GetAllAdminsQuery : IRequest<IList<AdminResponse>>
{
}