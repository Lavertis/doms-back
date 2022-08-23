namespace DoctorsOffice.Domain.DTO.Requests;

public class AuthenticateRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}