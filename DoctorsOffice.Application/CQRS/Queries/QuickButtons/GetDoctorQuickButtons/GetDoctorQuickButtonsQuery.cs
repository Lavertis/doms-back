using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.QuickButtons.GetDoctorQuickButtons;

public class GetDoctorQuickButtonsQuery : IRequest<HttpResult<DoctorQuickButtonsResponse>>
{
    public readonly Guid DoctorId;

    public GetDoctorQuickButtonsQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}