using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllDoctors;

public class GetAllDoctorsQuery : IRequest<IList<DoctorResponse>>
{
}