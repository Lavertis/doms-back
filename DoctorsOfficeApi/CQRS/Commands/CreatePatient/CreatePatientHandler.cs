using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.UserService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreatePatient;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, PatientResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;

    public CreatePatientHandler(AppDbContext dbContext, IUserService userService)
    {
        _dbContext = dbContext;
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

        var patientEntity = (await _dbContext.Patients.AddAsync(newPatient, cancellationToken)).Entity;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PatientResponse(patientEntity);
    }
}