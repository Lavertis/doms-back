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

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;

public class CreateDoctorHandler : IRequestHandler<CreateDoctorCommand, HttpResult<DoctorResponse>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public CreateDoctorHandler(IDoctorRepository doctorRepository, IUserService userService, IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<HttpResult<DoctorResponse>> Handle(CreateDoctorCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<DoctorResponse>();

        var createUserResult = await _userService.CreateUserAsync(new CreateUserRequest
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            RoleName = Roles.Doctor
        });
        if (createUserResult.IsError || createUserResult.HasValidationErrors)
        {
            if (createUserResult.HasValidationErrors)
                return result.WithValidationErrors(createUserResult.ValidationErrors);
            return result.WithError(createUserResult.Error).WithStatusCode(createUserResult.StatusCode);
        }

        var newAppUser = createUserResult.Value!;
        var newDoctor = new Doctor {AppUser = newAppUser};

        var doctorEntity = await _doctorRepository.CreateAsync(newDoctor);
        var doctorResponse = _mapper.Map<DoctorResponse>(doctorEntity);
        return result
            .WithValue(doctorResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }
}