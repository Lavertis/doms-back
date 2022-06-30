namespace DoctorsOfficeApi.Models.Requests;

public class RevokeRefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}