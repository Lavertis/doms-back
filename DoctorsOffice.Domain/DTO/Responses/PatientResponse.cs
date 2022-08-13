using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.DTO.Responses;

public class PatientResponse
{
    public PatientResponse()
    {
    }

    public PatientResponse(Patient patient)
    {
        Id = patient.Id;
        UserName = patient.AppUser.UserName;
        Email = patient.AppUser.Email;
        PhoneNumber = patient.AppUser.PhoneNumber;
        FirstName = patient.FirstName;
        LastName = patient.LastName;
        Address = patient.Address;
        DateOfBirth = patient.DateOfBirth;
    }

    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}