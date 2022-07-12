using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreatePatient;

public class CreatePatientCommand : IRequest<PatientResponse>
{
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string Password { get; set; } = default!;

    public CreatePatientCommand()
    {
    }

    public CreatePatientCommand(CreatePatientRequest request)
    {
        UserName = request.UserName;
        FirstName = request.FirstName;
        LastName = request.LastName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Address = request.Address;
        DateOfBirth = request.DateOfBirth.Date;
        Password = request.Password;
    }
}