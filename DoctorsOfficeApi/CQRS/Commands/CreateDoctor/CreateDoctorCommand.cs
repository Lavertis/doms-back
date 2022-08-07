using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreateDoctor;

public class CreateDoctorCommand : IRequest<DoctorResponse>
{
    public readonly string Email;
    public readonly string Password;
    public readonly string PhoneNumber;
    public readonly string UserName;

    public CreateDoctorCommand(CreateDoctorRequest request)
    {
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.Password;
    }
}