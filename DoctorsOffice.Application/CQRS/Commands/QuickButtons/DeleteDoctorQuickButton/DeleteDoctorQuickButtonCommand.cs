using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.QuickButtons.DeleteDoctorQuickButton;

public class DeleteDoctorQuickButtonCommand : IRequest<HttpResult<Unit>>
{
    public Guid DoctorId { get; set; }
    public Guid QuickButtonId { get; set; }
}