using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Models.Responses;

public class DoctorResponse
{
    public Guid Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;

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
}