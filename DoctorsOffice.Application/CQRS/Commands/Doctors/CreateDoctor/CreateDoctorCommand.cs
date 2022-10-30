using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;

public class CreateDoctorCommand : IRequest<HttpResult<CreateDoctorResponse>>
{
    public readonly string Email;
    public readonly string FirstName;
    public readonly string LastName;
    public readonly string PhoneNumber;

    public CreateDoctorCommand(CreateDoctorRequest request)
    {
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        FirstName = request.FirstName;
        LastName = request.LastName;
    }
}