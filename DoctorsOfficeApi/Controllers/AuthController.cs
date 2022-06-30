using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "Auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns a pair of access token and refresh token by username and password.
    /// </summary>
    [HttpPost("authenticate")]
    public async Task<ActionResult<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest request)
    {
        var response = await _authService.AuthenticateAsync(request, IpAddress());
        return Ok(response);
    }
    
    /// <summary>
    /// Returns a new pair of access token and refresh token by current refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthenticateResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = request.RefreshToken;
        var response = await _authService.RefreshTokenAsync(refreshToken, IpAddress());
        return Ok(response);
    }
    
    /// <summary>
    /// Revokes specified refresh token.
    /// </summary>
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeTokenAsync(RevokeRefreshTokenRequest request)
    {
        var refreshToken = request.RefreshToken;
        await _authService.RevokeRefreshTokenAsync(refreshToken, IpAddress());
        return Ok(new {message = "Token revoked"});
    }
    
    private string? IpAddress()
    {
        // get source ip address for the current request
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? null;
    }
}