namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateDoctorRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}