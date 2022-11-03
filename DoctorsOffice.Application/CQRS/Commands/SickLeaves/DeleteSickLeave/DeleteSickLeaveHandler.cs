using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.SickLeaves.DeleteSickLeave;

public class DeleteSickLeaveHandler : IRequestHandler<DeleteSickLeaveCommand, HttpResult<Unit>>
{
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public DeleteSickLeaveHandler(ISickLeaveRepository sickLeaveRepository)
    {
        _sickLeaveRepository = sickLeaveRepository;
    }

    public async Task<HttpResult<Unit>> Handle(DeleteSickLeaveCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var sickLeave = await _sickLeaveRepository.GetByIdAsync(request.SickLeaveId);
        if (sickLeave == null)
        {
            return result
                .WithError(new Error {Message = $"Sick leave with id {request.SickLeaveId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        if (sickLeave.DoctorId != request.DoctorId)
        {
            return result
                .WithError(new Error {Message = "You are not allowed to delete this sick leave"})
                .WithStatusCode(StatusCodes.Status403Forbidden);
        }

        await _sickLeaveRepository.DeleteByIdAsync(request.SickLeaveId);
        return result.WithValue(Unit.Value);
    }
}