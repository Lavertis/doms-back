using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserService _userService;

    public CreatePatientHandler(IPatientRepository patientRepository, IUserService userService, IMapper mapper)
    {
        _patientRepository = patientRepository;
        _userService = userService;
        _mapper = mapper;
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
            RoleName = Roles.Patient
        });
        if (createUserResult.IsError || createUserResult.HasValidationErrors)
        {
            if (createUserResult.HasValidationErrors)
                return result.WithValidationErrors(createUserResult.ValidationErrors);
            return result.WithError(createUserResult.Error).WithStatusCode(createUserResult.StatusCode);
        }

        var newAppUser = createUserResult.Value!;
        var newPatient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            NationalId = request.NationalId,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            AppUser = newAppUser
        };

        var patientEntity = await _patientRepository.CreateAsync(newPatient);
        var patientResponse = _mapper.Map<PatientResponse>(patientEntity);

        return result
            .WithValue(patientResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }
}