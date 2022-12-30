using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GenerateSickLeavePdf;

public class GenerateSickLeavePdfQuery : IRequest<HttpResult<FileResult>>
{
    public Guid SickLeaveId { get; set; }
    public Guid AppUserId { get; set; }
    public string Role { get; set; } = null!;
}