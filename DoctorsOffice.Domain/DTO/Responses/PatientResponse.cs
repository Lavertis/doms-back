﻿namespace DoctorsOffice.Domain.DTO.Responses;

public class PatientResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string NationalId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
}