using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Timetables.UpdateTimetables;

public class UpdateTimetablesCommand : IRequest<HttpResult<IEnumerable<TimetableResponse>>>
{
    public readonly IEnumerable<UpdateTimetableBatchRequest> Data;

    public UpdateTimetablesCommand(IEnumerable<UpdateTimetableBatchRequest> data)
    {
        Data = data;
    }
}