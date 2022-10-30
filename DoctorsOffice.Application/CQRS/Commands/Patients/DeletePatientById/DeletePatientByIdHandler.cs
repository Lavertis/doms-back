using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.DeletePatientById;

public class DeletePatientByIdHandler : IRequestHandler<DeletePatientByIdCommand, HttpResult<Unit>>
{
    private readonly AppUserManager _appUserManager;

    public DeletePatientByIdHandler(AppUserManager appUserManager)
    {
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<Unit>> Handle(DeletePatientByIdCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var findUserResult = await _appUserManager.FindByIdAsync(request.PatientId);
        if (findUserResult.IsError || findUserResult.Value == null)
        {
            return result
                .WithError(findUserResult.Error)
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var userIsPatient = await _appUserManager.IsInRoleAsync(findUserResult.Value, "Patient");
        if (!userIsPatient)
        {
            return result
                .WithError(new Error {Message = $"Patient with id {request.PatientId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var deleteUserResult = await _appUserManager.DeleteByIdAsync(request.PatientId);
        if (deleteUserResult.IsError)
        {
            return result
                .WithError(deleteUserResult.Error)
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(Unit.Value);
    }
}