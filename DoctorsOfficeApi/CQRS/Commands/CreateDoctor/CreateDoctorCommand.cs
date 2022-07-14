using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreateDoctor;

public class CreateDoctorCommand : IRequest<DoctorResponse>
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Password { get; set; } = default!;

    public CreateDoctorCommand()
    {
    }

    public CreateDoctorCommand(CreateDoctorRequest request)
    {
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.Password;
    }
}