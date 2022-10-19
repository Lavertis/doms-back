using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;

public class CreateDoctorCommand : IRequest<HttpResult<DoctorResponse>>
{
    public readonly string Email;
    public readonly string FirstName;
    public readonly string LastName;
    public readonly string Password;
    public readonly string PhoneNumber;
    public readonly string UserName;

    public CreateDoctorCommand(CreateDoctorRequest request)
    {
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.Password;
        FirstName = request.FirstName;
        LastName = request.LastName;
    }
}