using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.DTO.Responses;

public class DoctorResponse
{
    public DoctorResponse()
    {
    }

    public DoctorResponse(Doctor doctor)
    {
        Id = doctor.Id;
        UserName = doctor.AppUser.UserName;
        Email = doctor.AppUser.Email;
        PhoneNumber = doctor.AppUser.PhoneNumber;
    }

    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}