namespace DoctorsOffice.Domain.DTO.Requests;

public class AuthenticateRequest
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}