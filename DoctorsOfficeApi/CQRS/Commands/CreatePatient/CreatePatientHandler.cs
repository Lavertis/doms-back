using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PatientRepository;
using DoctorsOfficeApi.Services.UserService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreatePatient;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, PatientResponse>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserService _userService;


    public CreatePatientHandler(IPatientRepository patientRepository, IUserService userService)
    {
        _patientRepository = patientRepository;
        _userService = userService;
    }

    public async Task<PatientResponse> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var newUser = await _userService.CreateUserAsync(new CreateUserRequest
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            ConfirmPassword = request.Password,
            RoleName = RoleTypes.Patient
        });
        var newPatient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            AppUser = newUser
        };

        var patientEntity = await _patientRepository.CreateAsync(newPatient);

        return new PatientResponse(patientEntity);
    }
}