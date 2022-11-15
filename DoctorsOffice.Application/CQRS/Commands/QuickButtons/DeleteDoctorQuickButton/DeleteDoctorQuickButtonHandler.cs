using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.QuickButtons.DeleteDoctorQuickButton;

public class DeleteDoctorQuickButtonHandler : IRequestHandler<DeleteDoctorQuickButtonCommand, HttpResult<Unit>>
{
    private readonly IQuickButtonRepository _quickButtonRepository;

    public DeleteDoctorQuickButtonHandler(IQuickButtonRepository quickButtonRepository)
    {
        _quickButtonRepository = quickButtonRepository;
    }

    public async Task<HttpResult<Unit>> Handle(DeleteDoctorQuickButtonCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var quickButton = await _quickButtonRepository.GetByIdAsync(request.QuickButtonId);
        if (quickButton == null)
        {
            return result
                .WithError(new Error {Message = "Quick button not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        if (quickButton.DoctorId != request.DoctorId)
        {
            return result
                .WithError(new Error {Message = "Quick button is not owned by this doctor"})
                .WithStatusCode(StatusCodes.Status403Forbidden);
        }

        await _quickButtonRepository.DeleteByIdAsync(quickButton.Id);
        return result;
    }
}