using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOffice.Application.CQRS.Commands.UpdatePatientById;

public class UpdatePatientByIdHandler : IRequestHandler<UpdatePatientByIdCommand, PatientResponse>
{
    private readonly IPatientRepository _patientRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;

    public UpdatePatientByIdHandler(IPatientRepository patientRepository, IUserService userService,
        UserManager<AppUser> userManager)
    {
        _patientRepository = patientRepository;
        _userService = userService;
        _userManager = userManager;
    }

    public async Task<PatientResponse> Handle(UpdatePatientByIdCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.PatientId);
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
        await _patientRepository.UpdateByIdAsync(request.PatientId, patient);

        patient.AppUser = appUser;
        return new PatientResponse(patient);
    }
}