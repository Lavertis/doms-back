using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByDoctorId;

public class GetPrescriptionsByDoctorIdQuery : IRequest<IList<PrescriptionResponse>>
{
    public Guid DoctorId { get; set; }

    public GetPrescriptionsByDoctorIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}