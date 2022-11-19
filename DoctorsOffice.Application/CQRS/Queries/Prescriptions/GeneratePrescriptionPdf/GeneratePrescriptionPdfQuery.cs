using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GeneratePrescriptionPdf;

public class GeneratePrescriptionPdfQuery : IRequest<HttpResult<FileResult>>
{
    public Guid PrescriptionId { get; set; }
    public Guid AppUserId { get; set; }
    public string Role { get; set; } = null!;
}