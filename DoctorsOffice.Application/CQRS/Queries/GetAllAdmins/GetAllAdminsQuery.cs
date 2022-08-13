using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetAllAdmins;

public class GetAllAdminsQuery : IRequest<IList<AdminResponse>>
{
}