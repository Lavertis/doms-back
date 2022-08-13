using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.CreateDoctor;

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
                RoleName = RoleTypes.Doctor
            })
        };

        var doctorEntity = await _doctorRepository.CreateAsync(newDoctor);
        return new DoctorResponse(doctorEntity);
    }
}