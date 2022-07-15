﻿using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Models.Responses;

public class AdminResponse
{
    public string Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;

    public AdminResponse()
    {
    }

    public AdminResponse(Admin admin)
    {
        Id = admin.Id;
        UserName = admin.AppUser.UserName;
        Email = admin.AppUser.Email;
    }
}