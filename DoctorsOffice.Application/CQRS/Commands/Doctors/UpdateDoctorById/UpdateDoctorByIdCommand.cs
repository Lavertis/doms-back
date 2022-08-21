using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;

public class UpdateDoctorByIdCommand : IRequest<HttpResult<DoctorResponse>>
{
    public readonly Guid DoctorId;
    public readonly string? Email;
    public readonly string? NewPassword;
    public readonly string? PhoneNumber;
    public readonly string? UserName;

    public UpdateDoctorByIdCommand(UpdateAuthenticatedDoctorRequest request, Guid doctorId)
    {
        DoctorId = doctorId;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        NewPassword = request.NewPassword;
    }

    public UpdateDoctorByIdCommand(UpdateDoctorRequest request, Guid doctorId)
    {
        DoctorId = doctorId;
        UserName = request.UserName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        NewPassword = request.NewPassword;
    }
}