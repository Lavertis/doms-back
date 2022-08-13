namespace DoctorsOffice.Domain.DTO.Responses;

public class AuthenticateResponse
{
    public string JwtToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}