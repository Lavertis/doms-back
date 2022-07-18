using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using DoctorsOfficeApi.Services.UserService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreateDoctor;

public class CreateDoctorHandler : IRequestHandler<CreateDoctorCommand, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUserService _userService;

    public CreateDoctorHandler(IDoctorRepository doctorRepository, IUserService userService)
    {
        _doctorRepository = doctorRepository;
        _userService = userService;
    }

    public async Task<DoctorResponse> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var newDoctor = new Doctor
        {
            AppUser = await _userService.CreateUserAsync(new CreateUserRequest
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                ConfirmPassword = request.Password,
                RoleName = RoleTypes.Doctor
            })
        };

        var doctorEntity = await _doctorRepository.CreateAsync(newDoctor);
        return new DoctorResponse(doctorEntity);
    }
}