using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Timetables.GetTimetablesByDoctorId;

public class GetTimetablesByDoctorIdQuery : IRequest<HttpResult<IEnumerable<TimetableResponse>>>
{
    public Guid DoctorId;
    public DateTime? EndDateTime;
    public DateTime? StartDateTime;
}