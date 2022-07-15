﻿namespace DoctorsOfficeApi.Config;

public class AppSettings
{
    public string JwtSecretKey { get; set; } = default!;

    // in days
    public int RefreshTokenTtl { get; set; }
}