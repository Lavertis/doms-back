using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Roles = RoleTypes.Admin)]
[ApiExplorerSettings(GroupName = "User")]
public class UserController : Controller
{
    private IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Returns all base users. Only for admins.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<UserResponse>>> GetAllUsersAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        var responses = users.Select(u => new UserResponse(u)).ToList();
        return Ok(responses);
    }

    /// <summary>
    /// Returns base user by id. Only for admins.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<IList<UserResponse>>> GetUserByIdAsync(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        var response = new UserResponse(user);
        return Ok(response);
    }

    /// <summary>
    /// Returns refresh token for user by id. Only for admins.
    /// </summary>
    [HttpGet("{id}/refresh-tokens")]
    public async Task<ActionResult<IList<RefreshToken>>> GetRefreshTokensByUserId(string id)
    {
        var refreshTokens = await _userService.GetUserRefreshTokensAsync(id);
        return Ok(refreshTokens);
    }
}