using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.DeletePrescription;

public class DeletePrescriptionCommand : IRequest<HttpResult<Unit>>
{
    public Guid DoctorId { get; set; }
    public Guid PrescriptionId { get; set; }
}