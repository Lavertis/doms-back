using DoctorsOffice.Domain.Entities.UserTypes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Infrastructure.Identity.TokenProviders;

public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : AppUser
{
    public EmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
        IOptions<EmailConfirmationTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
}