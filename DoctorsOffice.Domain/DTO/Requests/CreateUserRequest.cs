namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateUserRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
}