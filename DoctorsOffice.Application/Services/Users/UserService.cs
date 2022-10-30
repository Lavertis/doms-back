using AutoMapper;
using DoctorsOffice.Application.Validation;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.Services.Users;

public class UserService : IUserService
{
    private readonly AppRoleManager _appRoleManager;
    private readonly AppUserManager _appUserManager;
    private readonly IMapper _mapper;

    public UserService(AppUserManager appUserManager, AppRoleManager appRoleManager, IMapper mapper)
    {
        _appUserManager = appUserManager;
        _appRoleManager = appRoleManager;
        _mapper = mapper;
    }

    public async Task<HttpResult<AppUser>> CreateUserAsync(CreateUserRequest request)
    {
        var result = new HttpResult<AppUser>();

        var validationResult =
            await new CreateUserRequestValidator(_appUserManager, _appRoleManager).ValidateAsync(request);
        if (!validationResult.IsValid)
            return result.WithValidationErrors(validationResult.Errors);

        var user = _mapper.Map<AppUser>(request);
        user.EmailConfirmed = request.EmailConfirmed;
        user.UserName = request.Email;
        user.NormalizedUserName = request.Email.ToUpper();
        user.NormalizedEmail = request.Email.ToUpper();

        var createUserIdentityResult = await _appUserManager.CreateAsync(user, request.Password);
        if (!createUserIdentityResult.Succeeded)
        {
            return result
                .WithError(new Error {Message = "UserManager could not create user"})
                .WithStatusCode(StatusCodes.Status500InternalServerError);
        }

        var addToRoleIdentityResult = await _appUserManager.AddToRoleAsync(user, request.RoleName);
        if (!addToRoleIdentityResult.Succeeded)
        {
            return result
                .WithError(new Error {Message = "UserManager could not add user to role"})
                .WithStatusCode(StatusCodes.Status500InternalServerError);
        }

        return result.WithValue(user).WithStatusCode(StatusCodes.Status201Created);
    }

    public async Task<HttpResult<AppUser>> UpdateUserByIdAsync(Guid userId, UpdateUserRequest request)
    {
        var result = new HttpResult<AppUser>();

        var validationResult = await new UpdateUserRequestValidator(_appUserManager).ValidateAsync(request);
        if (!validationResult.IsValid)
            return result.WithValidationErrors(validationResult.Errors);

        var findByIdResult = await _appUserManager.FindByIdAsync(userId);
        if (findByIdResult.IsError || findByIdResult.Value is null)
        {
            return result
                .WithError(new Error {Message = "User with requested id does not exist"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var user = findByIdResult.Value;
        if (request.Email is not null)
        {
            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpper();
            user.UserName = request.Email;
            user.NormalizedUserName = request.Email.ToUpper();
        }

        if (request.PhoneNumber is not null)
            user.PhoneNumber = request.PhoneNumber;
        if (request.NewPassword is not null)
            user.PasswordHash = _appUserManager.PasswordHasher.HashPassword(user, request.NewPassword);

        var updateUserIdentityResult = await _appUserManager.UpdateAsync(user);
        if (!updateUserIdentityResult.Succeeded)
        {
            return result
                .WithError(new Error {Message = "UserManager could not update user"})
                .WithStatusCode(StatusCodes.Status500InternalServerError);
        }

        return result.WithValue(user);
    }
}