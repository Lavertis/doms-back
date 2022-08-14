using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.UpdatePatientById;

public class UpdatePatientByIdCommand : IRequest<PatientResponse>
{
    public readonly string? Address;
    public readonly string CurrentPassword;
    public readonly DateTime? DateOfBirth;
    public readonly string? Email;
    public readonly string? FirstName;
    public readonly string? LastName;
    public readonly string? NewPassword;
    public readonly Guid PatientId;
    public readonly string? PhoneNumber;
    public readonly string? UserName;

    public UpdatePatientByIdCommand(UpdateAuthenticatedPatientRequest request, Guid patientId)
    {
        PatientId = patientId;
        UserName = request.UserName;
        FirstName = request.FirstName;
        LastName = request.LastName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Address = request.Address;
        DateOfBirth = request.DateOfBirth?.Date;
        NewPassword = request.NewPassword;
        CurrentPassword = request.CurrentPassword;
    }
}