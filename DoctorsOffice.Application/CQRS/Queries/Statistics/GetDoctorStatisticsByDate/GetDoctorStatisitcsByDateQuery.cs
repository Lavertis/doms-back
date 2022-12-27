using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Statistics.GetDoctorStatisticsByDate;

public class GetDoctorStatisitcsByDateQuery : IRequest<HttpResult<DoctorStatisticsResponse>>
{
    public DateTime? DateEnd { get; set; }
    public DateTime? DateStart { get; set; }
    public Guid? DoctorId { get; set; }
}