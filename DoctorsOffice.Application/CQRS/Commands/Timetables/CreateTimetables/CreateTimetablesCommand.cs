using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Timetables.CreateTimetables;

public class CreateTimetablesCommand : IRequest<HttpResult<IEnumerable<TimetableResponse>>>
{
    public IEnumerable<CreateTimetableRequest> Data { get; set; } = default!;
    public Guid DoctorId { get; set; }
}