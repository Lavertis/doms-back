using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using DoctorsOfficeApi.Services.UserService;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.CQRS.Commands.UpdateDoctorById;

public class UpdateDoctorByIdHandler : IRequestHandler<UpdateDoctorByIdCommand, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;

    public UpdateDoctorByIdHandler(IDoctorRepository doctorRepository, IUserService userService,
        UserManager<AppUser> userManager)
    {
        _doctorRepository = doctorRepository;
        _userService = userService;
        _userManager = userManager;
    }

    public async Task<DoctorResponse> Handle(UpdateDoctorByIdCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        var appUser = _userManager.Users.First(x => x.Id == doctor.Id);

        appUser.UserName = request.UserName ?? appUser.UserName;
        appUser.Email = request.Email ?? appUser.Email;
        appUser.PhoneNumber = request.PhoneNumber ?? appUser.PhoneNumber;
        if (!string.IsNullOrEmpty(request.Password))
        {
            _userService.SetUserPassword(appUser, request.Password!);
        }

        await _userManager.UpdateAsync(appUser);

        doctor.AppUser = appUser;
        return new DoctorResponse(doctor);
    }
}