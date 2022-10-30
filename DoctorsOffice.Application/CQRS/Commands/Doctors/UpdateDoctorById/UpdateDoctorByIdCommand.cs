using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;

public class UpdateDoctorByIdCommand : IRequest<HttpResult<DoctorResponse>>
{
    public readonly Guid DoctorId;
    public readonly string? Email;
    public readonly string? FirstName;
    public readonly string? LastName;
    public readonly string? NewPassword;
    public readonly string? PhoneNumber;

    public UpdateDoctorByIdCommand(UpdateAuthenticatedDoctorRequest request, Guid doctorId)
    {
        DoctorId = doctorId;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        NewPassword = request.NewPassword;
        FirstName = request.FirstName;
        LastName = request.LastName;
    }

    public UpdateDoctorByIdCommand(UpdateDoctorRequest request, Guid doctorId)
    {
        DoctorId = doctorId;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        NewPassword = request.NewPassword;
        FirstName = request.FirstName;
        LastName = request.LastName;
    }
}