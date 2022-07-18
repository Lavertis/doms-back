using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PatientRepository;
using DoctorsOfficeApi.Services.UserService;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.CQRS.Commands.UpdatePatientById;

public class UpdatePatientByIdHandler : IRequestHandler<UpdatePatientByIdCommand, PatientResponse>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserService _userService;
    private readonly UserManager<AppUser> _userManager;

    public UpdatePatientByIdHandler(IPatientRepository patientRepository, IUserService userService, UserManager<AppUser> userManager)
    {
        _patientRepository = patientRepository;
        _userService = userService;
        _userManager = userManager;
    }

    public async Task<PatientResponse> Handle(UpdatePatientByIdCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id);
        var appUser = _userManager.Users.First(x => x.Id == patient.Id);

        appUser.UserName = request.UserName ?? appUser.UserName;
        appUser.Email = request.Email ?? appUser.Email;
        appUser.PhoneNumber = request.PhoneNumber ?? appUser.PhoneNumber;
        if (!string.IsNullOrEmpty(request.NewPassword))
            _userService.SetUserPassword(appUser, request.NewPassword!);

        patient.FirstName = request.FirstName ?? patient.FirstName;
        patient.LastName = request.LastName ?? patient.LastName;
        patient.Address = request.Address ?? patient.Address;
        patient.DateOfBirth = request.DateOfBirth ?? patient.DateOfBirth;

        await _userManager.UpdateAsync(appUser);
        await _patientRepository.UpdateByIdAsync(request.Id, patient);

        patient.AppUser = appUser;
        return new PatientResponse(patient);
    }
}