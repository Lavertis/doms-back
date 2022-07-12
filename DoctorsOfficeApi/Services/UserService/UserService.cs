using System.Security.Claims;
using AutoMapper;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Services.UserService;

public class UserService : IUserService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public UserService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<AppUser> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User with requested id does not exist");
        return user;
    }

    public async Task<AppUser> GetUserByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            throw new NotFoundException("User with requested username does not exist");
        return user;
    }

    public async Task<AppUser> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            await new CreateUserRequestValidator(this, _roleManager).ValidateAndThrowAsync(request);
        }
        catch (ValidationException e)
        {
            throw new BadRequestException(e.Message);
        }

        var mapper = new Mapper(new MapperConfiguration(cfg =>
            cfg.CreateMap<CreateUserRequest, AppUser>()
        ));
        var user = mapper.Map<AppUser>(request);
        user.NormalizedUserName = user.UserName.ToUpper();
        user.NormalizedEmail = user.Email.ToUpper();

        var createUserIdentityResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserIdentityResult.Succeeded)
            throw new AppException("UserManager could not create user");

        var addToRoleIdentityResult = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!addToRoleIdentityResult.Succeeded)
            throw new AppException("UserManager could not add user to role");

        return user;
    }

    public async Task<AppUser> UpdateUserByIdAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            await new UpdateUserRequestValidator(this).ValidateAndThrowAsync(request);
        }
        catch (ValidationException e)
        {
            throw new BadRequestException(e.Message);
        }

        var user = await GetUserByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User with requested id does not exist");

        if (request.UserName != null)
        {
            user.UserName = request.UserName;
            user.NormalizedUserName = request.UserName.ToUpper();
        }

        if (request.Email != null)
        {
            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpper();
        }

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;
        if (request.NewPassword != null)
            SetUserPassword(user, request.NewPassword);

        var updateUserIdentityResult = await _userManager.UpdateAsync(user);
        if (!updateUserIdentityResult.Succeeded)
            throw new AppException("UserManager could not update user");

        return user;
    }

    public async Task<bool> DeleteUserByIdAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        var deleteIdentityResult = await _userManager.DeleteAsync(user);
        return deleteIdentityResult.Succeeded;
    }

    public async Task<IList<Claim>> GetUserRolesAsClaimsAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id),
            new(ClaimTypes.Name, user.UserName),
        };
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        roles.ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));
        return claims;
    }

    public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        var exists = await _userManager.Users.AnyAsync(user =>
                user.NormalizedUserName == userName.ToUpper(), cancellationToken: cancellationToken
        );
        return exists;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.AnyAsync(user =>
                user.NormalizedEmail == email.ToUpper(), cancellationToken: cancellationToken
        );
    }

    public async Task<bool> ValidateUserPasswordAsync(string userName, string password)
    {
        var user = await GetUserByUserNameAsync(userName);
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        return isPasswordValid;
    }

    public void SetUserPassword(AppUser user, string newPassword)
    {
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, newPassword);
    }

    public async Task<bool> UserExistsByIdAsync(string userId)
    {
        return await _userManager.Users.AnyAsync(user => user.Id == userId);
    }
}