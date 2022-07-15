using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.PatientService;
using DoctorsOfficeApi.Services.UserService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdatePatientById;

public class UpdatePatientByIdHandler : IRequestHandler<UpdatePatientByIdCommand, PatientResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IPatientService _patientService;
    private readonly IUserService _userService;

    public UpdatePatientByIdHandler(
        AppDbContext dbContext,
        IPatientService patientService,
        IUserService userService)
    {
        _dbContext = dbContext;
        _patientService = patientService;
        _userService = userService;
    }

    public async Task<PatientResponse> Handle(UpdatePatientByIdCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetPatientByIdAsync(request.Id);

        if (request.UserName is not null)
        {
            patient.AppUser.UserName = request.UserName;
            patient.AppUser.NormalizedUserName = request.UserName.ToUpper();
        }

        if (request.Email is not null)
        {
            patient.AppUser.Email = request.Email;
            patient.AppUser.NormalizedEmail = request.Email.ToUpper();
        }

        patient.FirstName = request.FirstName ?? patient.FirstName;
        patient.LastName = request.LastName ?? patient.LastName;
        patient.AppUser.PhoneNumber = request.PhoneNumber ?? patient.PhoneNumber;
        patient.Address = request.Address ?? patient.Address;
        patient.DateOfBirth = request.DateOfBirth ?? patient.DateOfBirth;
        if (!string.IsNullOrEmpty(request.NewPassword))
            _userService.SetUserPassword(patient.AppUser, request.NewPassword!);

        _dbContext.Patients.Update(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PatientResponse(patient);
    }
}