using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Statistics.GetAdminStatisticsByDate;

public class GetAdminStatisitcsByDateQuery : IRequest<HttpResult<AdminStatisticsResponse>>
{
    public DateTime? DateEnd { get; set; }
    public DateTime? DateStart { get; set; }
    public Guid? DoctorId { get; set; }
}