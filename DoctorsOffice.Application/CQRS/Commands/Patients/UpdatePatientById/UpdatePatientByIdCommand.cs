using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.UpdatePatientById;

public class UpdatePatientByIdCommand : IRequest<HttpResult<PatientResponse>>
{
    public readonly string? Address;
    public readonly string CurrentPassword;
    public readonly DateTime? DateOfBirth;
    public readonly string? Email;
    public readonly string? FirstName;
    public readonly string? LastName;
    public readonly string? NationalId;
    public readonly string? NewPassword;
    public readonly Guid PatientId;
    public readonly string? PhoneNumber;

    public UpdatePatientByIdCommand(UpdateAuthenticatedPatientRequest request, Guid patientId)
    {
        PatientId = patientId;
        FirstName = request.FirstName;
        LastName = request.LastName;
        NationalId = request.NationalId;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Address = request.Address;
        DateOfBirth = request.DateOfBirth?.Date;
        NewPassword = request.NewPassword;
        CurrentPassword = request.CurrentPassword;
    }
}