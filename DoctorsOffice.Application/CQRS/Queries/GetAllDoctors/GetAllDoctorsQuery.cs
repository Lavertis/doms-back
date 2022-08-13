using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetAllDoctors;

public class GetAllDoctorsQuery : IRequest<IList<DoctorResponse>>
{
}