using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.DeleteDoctorById;

public class DeleteDoctorByIdHandler : IRequestHandler<DeleteDoctorByIdCommand, HttpResult<Unit>>
{
    private readonly AppUserManager _appUserManager;

    public DeleteDoctorByIdHandler(AppUserManager appUserManager)
    {
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<Unit>> Handle(DeleteDoctorByIdCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var findUserResult = await _appUserManager.FindByIdAsync(request.DoctorId);
        if (findUserResult.IsError || findUserResult.Value == null)
        {
            return result
                .WithError(findUserResult.Error)
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var userIsDoctor = await _appUserManager.IsInRoleAsync(findUserResult.Value, "Doctor");
        if (!userIsDoctor)
        {
            return result
                .WithError(new Error {Message = $"Doctor with id {request.DoctorId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var deleteUserResult = await _appUserManager.DeleteByIdAsync(request.DoctorId);
        if (deleteUserResult.IsError)
        {
            return result
                .WithError(deleteUserResult.Error)
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(Unit.Value);
    }
}