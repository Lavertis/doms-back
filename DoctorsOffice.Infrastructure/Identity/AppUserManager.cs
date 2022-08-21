using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Infrastructure.Identity;

public class AppUserManager : UserManager<AppUser>
{
    public AppUserManager(
        IUserStore<AppUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<AppUser> passwordHasher,
        IEnumerable<IUserValidator<AppUser>> userValidators,
        IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
        ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
        IServiceProvider services, ILogger<AppUserManager> logger)
        : base(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    {
    }

    public virtual async Task<CommonResult<AppUser>> FindByIdAsync(Guid userId)
    {
        var result = new CommonResult<AppUser>();
        var userIdAsString = userId.ToString();
        var user = await FindByIdAsync(userIdAsString);
        if (user is not null)
            return result.WithValue(user);
        return result.WithError(new Error {Message = $"{nameof(AppUser)} with id {userIdAsString} does not exist"});
    }

    public new virtual async Task<CommonResult<AppUser>> FindByNameAsync(string userName)
    {
        var result = new CommonResult<AppUser>();
        var user = await base.FindByNameAsync(userName);
        if (user is not null)
            return result.WithValue(user);
        return result.WithError(new Error {Message = $"{nameof(AppUser)} with username {userName} does not exist"});
    }

    public new virtual async Task<CommonResult<AppUser>> FindByEmailAsync(string email)
    {
        var result = new CommonResult<AppUser>();
        var user = await base.FindByEmailAsync(email);
        if (user is not null)
            return result.WithValue(user);
        return result.WithError(new Error {Message = $"{nameof(AppUser)} with email {email} does not exist"});
    }

    public virtual async Task<bool> ExistsByEmailAsync(string email)
    {
        return await base.FindByEmailAsync(email) is not null;
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid userId)
    {
        return await FindByIdAsync(userId.ToString()) is not null;
    }

    public virtual async Task<bool> ExistsByUserNameAsync(string userName)
    {
        return await base.FindByNameAsync(userName) is not null;
    }

    public virtual async Task<CommonResult<bool>> DeleteByIdAsync(Guid userId)
    {
        var result = new CommonResult<bool>();
        var findByIdResult = await FindByIdAsync(userId);
        if (findByIdResult.IsFailed || findByIdResult.Value is null)
            return result.WithError(findByIdResult.Error);

        var user = findByIdResult.Value;
        await DeleteAsync(user);
        return result.WithValue(true);
    }

    public virtual async Task<CommonResult<bool>> ValidatePasswordAsync(string userName, string password)
    {
        var result = new CommonResult<bool>();
        var findByNameResult = await FindByNameAsync(userName);
        if (findByNameResult.IsFailed || findByNameResult.Value is null)
            return result.WithError(findByNameResult.Error);

        var user = findByNameResult.Value;
        var isPasswordValid = await CheckPasswordAsync(user, password);
        return result.WithValue(isPasswordValid);
    }
}