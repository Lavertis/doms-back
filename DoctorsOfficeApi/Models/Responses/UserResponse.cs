using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Models.Responses;

public class UserResponse
{
    public string Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string NormalizedUserName { get; set; } = default!;
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }

    public UserResponse(AppUser user)
    {
        Id = user.Id;
        UserName = user.UserName;
        NormalizedUserName = user.NormalizedUserName;
        TwoFactorEnabled = user.TwoFactorEnabled;
        LockoutEnd = user.LockoutEnd;
        LockoutEnabled = user.LockoutEnabled;
        AccessFailedCount = user.AccessFailedCount;
    }

    public UserResponse()
    {
    }
}