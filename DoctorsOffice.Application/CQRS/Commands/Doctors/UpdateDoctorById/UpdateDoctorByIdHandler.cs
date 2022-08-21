using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;

public class UpdateDoctorByIdHandler : IRequestHandler<UpdateDoctorByIdCommand, HttpResult<DoctorResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IDoctorRepository _doctorRepository;

    public UpdateDoctorByIdHandler(IDoctorRepository doctorRepository, AppUserManager appUserManager)
    {
        _doctorRepository = doctorRepository;
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<DoctorResponse>> Handle(
        UpdateDoctorByIdCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<DoctorResponse>();

        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        if (doctor is null)
        {
            return result
                .WithError(new Error {Message = $"Doctor with id {request.DoctorId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var appUser = await _appUserManager.Users.FirstAsync(x => x.Id == doctor.Id, cancellationToken);

        appUser.UserName = request.UserName ?? appUser.UserName;
        appUser.Email = request.Email ?? appUser.Email;
        appUser.PhoneNumber = request.PhoneNumber ?? appUser.PhoneNumber;
        if (!string.IsNullOrEmpty(request.NewPassword))
            appUser.PasswordHash = _appUserManager.PasswordHasher.HashPassword(appUser, request.NewPassword);

        await _appUserManager.UpdateAsync(appUser);

        doctor.AppUser = appUser;
        return result.WithValue(new DoctorResponse(doctor));
    }
}