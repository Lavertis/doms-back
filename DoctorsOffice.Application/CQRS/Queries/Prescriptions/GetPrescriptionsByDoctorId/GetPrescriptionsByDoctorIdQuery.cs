using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByDoctorId;

public class GetPrescriptionsByDoctorIdQuery : IRequest<HttpResult<IEnumerable<PrescriptionResponse>>>
{
    public readonly Guid DoctorId;

    public GetPrescriptionsByDoctorIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}