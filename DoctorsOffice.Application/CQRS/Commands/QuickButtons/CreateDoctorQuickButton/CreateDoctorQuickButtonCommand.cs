using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.QuickButtons.CreateDoctorQuickButton;

public class CreateDoctorQuickButtonCommand : IRequest<HttpResult<QuickButtonResponse>>
{
    public CreateQuickButtonRequest Data { get; set; } = null!;
    public Guid DoctorId { get; set; }
}