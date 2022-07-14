using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdateDoctorById;

public class UpdateDoctorByIdCommand : IRequest<DoctorResponse>
{
    public string Id { get; set; } = default!;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    public UpdateDoctorByIdCommand()
    {
    }

    public UpdateDoctorByIdCommand(string id, UpdateAuthenticatedDoctorRequest request)
    {
        Id = id;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.NewPassword;
    }

    public UpdateDoctorByIdCommand(string id, UpdateDoctorRequest request)
    {
        Id = id;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.NewPassword;
    }
}