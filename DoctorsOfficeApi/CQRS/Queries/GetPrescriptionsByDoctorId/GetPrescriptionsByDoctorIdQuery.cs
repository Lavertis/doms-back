using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByDoctorId;

public class GetPrescriptionsByDoctorIdQuery : IRequest<IList<PrescriptionResponse>>
{
    public readonly Guid DoctorId;

    public GetPrescriptionsByDoctorIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}