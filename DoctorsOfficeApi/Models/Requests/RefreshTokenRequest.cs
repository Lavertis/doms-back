﻿namespace DoctorsOfficeApi.Models.Requests;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}