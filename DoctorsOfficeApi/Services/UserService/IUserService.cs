﻿using System.Security.Claims;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models.Requests;

namespace DoctorsOfficeApi.Services.UserService;

public interface IUserService
{
    Task<AppUser> GetUserByIdAsync(string userId);
    Task<AppUser> GetUserByUserNameAsync(string userName);
    Task<AppUser> CreateUserAsync(CreateUserRequest request);
    Task<AppUser> UpdateUserByIdAsync(string userId, UpdateUserRequest request);
    Task<bool> DeleteUserByIdAsync(string userId);
    Task<IList<Claim>> GetUserRolesAsClaimsAsync(AppUser user);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ValidateUserPasswordAsync(string userName, string password);
    void SetUserPassword(AppUser user, string newPassword);
    Task<bool> UserExistsByIdAsync(string userId);
}