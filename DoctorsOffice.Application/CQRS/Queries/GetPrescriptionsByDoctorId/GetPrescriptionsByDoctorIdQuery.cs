using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetPrescriptionsByDoctorId;

public class GetPrescriptionsByDoctorIdQuery : IRequest<IList<PrescriptionResponse>>
{
    public readonly Guid DoctorId;

    public GetPrescriptionsByDoctorIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}