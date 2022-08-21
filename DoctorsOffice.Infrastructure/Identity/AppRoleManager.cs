using DoctorsOffice.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DoctorsOffice.Infrastructure.Identity;

public class AppRoleManager : RoleManager<AppRole>
{
    public AppRoleManager(
        IRoleStore<AppRole> store,
        IEnumerable<IRoleValidator<AppRole>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<AppRoleManager> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
}