﻿namespace DoctorsOffice.Infrastructure.Config;

public class JwtSettings
{
    public string SecretKey { get; set; } = default!;
    public int TokenLifetimeInMinutes { get; set; }
    public int RefreshTokenLifetimeInDays { get; set; }
    public int RefreshTokenTtlInDays { get; set; }
}