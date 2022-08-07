namespace DoctorsOfficeApi.Config;

public class AppSettings
{
    public string JwtSecretKey { get; set; } = string.Empty;
    public int RefreshTokenTtlInDays { get; set; }
}