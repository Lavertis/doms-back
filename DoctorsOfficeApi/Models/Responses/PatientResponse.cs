﻿using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Models.Responses;

public class PatientResponse
{
    public Guid Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public DateTime DateOfBirth { get; set; } = default!;

    public PatientResponse()
    {
    }

    public PatientResponse(Patient patient)
    {
        Id = patient.Id;
        UserName = patient.AppUser.UserName;
        Email = patient.AppUser.Email;
        PhoneNumber = patient.AppUser.PhoneNumber;
        FirstName = patient.FirstName;
        LastName = patient.LastName;
        Address = patient.Address;
        DateOfBirth = patient.DateOfBirth;
    }
}