using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdateDoctorById;

public class UpdateDoctorByIdCommand : IRequest<DoctorResponse>
{
    public Guid Id { get; set; } = default!;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    public UpdateDoctorByIdCommand()
    {
    }

    public UpdateDoctorByIdCommand(Guid id, UpdateAuthenticatedDoctorRequest request)
    {
        Id = id;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.NewPassword;
    }

    public UpdateDoctorByIdCommand(Guid id, UpdateDoctorRequest request)
    {
        Id = id;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.NewPassword;
    }
}