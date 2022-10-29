using DoctorsOffice.Domain.Entities.UserTypes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Infrastructure.Identity.TokenProviders;

public class PasswordResetTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : AppUser
{
    public PasswordResetTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<PasswordResetTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
{
}