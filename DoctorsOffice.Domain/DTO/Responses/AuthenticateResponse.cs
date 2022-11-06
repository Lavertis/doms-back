using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Domain.DTO.Responses;

public class AuthenticateResponse
{
    public string JwtToken { get; set; } = null!;
    [JsonIgnore] public string RefreshToken { get; set; } = null!;
    [JsonIgnore] public CookieOptions CookieOptions { get; set; } = null!;
}