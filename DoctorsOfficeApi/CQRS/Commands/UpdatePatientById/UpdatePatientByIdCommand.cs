using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdatePatientById;

public class UpdatePatientByIdCommand : IRequest<PatientResponse>
{
    public string Id { get; set; } = default!;
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NewPassword { get; set; }
    public string CurrentPassword { get; set; } = default!;

    public UpdatePatientByIdCommand()
    {
    }

    public UpdatePatientByIdCommand(string id, UpdateAuthenticatedPatientRequest request)
    {
        Id = id;
        UserName = request.UserName;
        FirstName = request.FirstName;
        LastName = request.LastName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Address = request.Address;
        if (request.DateOfBirth != null)
            DateOfBirth = request.DateOfBirth.Value.Date;
        NewPassword = request.NewPassword;
        CurrentPassword = request.CurrentPassword;
    }
}