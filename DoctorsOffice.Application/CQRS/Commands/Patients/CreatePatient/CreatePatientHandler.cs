using DoctorsOffice.Application.Services.Users;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, HttpResult<PatientResponse>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserService _userService;


    public CreatePatientHandler(IPatientRepository patientRepository, IUserService userService)
    {
        _patientRepository = patientRepository;
        _userService = userService;
    }

    public async Task<HttpResult<PatientResponse>> Handle(
        CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<PatientResponse>();

        var createUserResult = await _userService.CreateUserAsync(new CreateUserRequest
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            RoleName = RoleTypes.Patient
        });
        if (createUserResult.IsFailed || createUserResult.Value is null)
        {
            if (createUserResult.ErrorField is not null && createUserResult.Error is not null)
            {
                return result
                    .WithFieldError(createUserResult.ErrorField, createUserResult.Error)
                    .WithStatusCode(createUserResult.StatusCode);
            }

            return result.WithError(createUserResult.Error).WithStatusCode(createUserResult.StatusCode);
        }

        var newAppUser = createUserResult.Value;
        var newPatient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            AppUser = newAppUser
        };

        var patientEntity = await _patientRepository.CreateAsync(newPatient);

        return result
            .WithValue(new PatientResponse(patientEntity))
            .WithStatusCode(StatusCodes.Status201Created);
    }
}