using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.UpdateDoctorById;

public class UpdateDoctorByIdCommand : IRequest<DoctorResponse>
{
    public readonly Guid DoctorId;
    public readonly string? Email;
    public readonly string? Password;
    public readonly string? PhoneNumber;
    public readonly string? UserName;

    public UpdateDoctorByIdCommand(UpdateAuthenticatedDoctorRequest request, Guid doctorId)
    {
        DoctorId = doctorId;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.NewPassword;
    }

    public UpdateDoctorByIdCommand(UpdateDoctorRequest request, Guid doctorId)
    {
        DoctorId = doctorId;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Password = request.NewPassword;
    }
}