﻿using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;

public class CreatePatientCommand : IRequest<HttpResult<PatientResponse>>
{
    public readonly string Address;
    public readonly DateTime DateOfBirth;
    public readonly string Email;
    public readonly string FirstName;
    public readonly string LastName;
    public readonly string NationalId;
    public readonly string Password;
    public readonly string PhoneNumber;
    public readonly string UserName;

    public CreatePatientCommand(CreatePatientRequest request)
    {
        UserName = request.UserName;
        FirstName = request.FirstName;
        LastName = request.LastName;
        NationalId = request.NationalId;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Address = request.Address;
        DateOfBirth = request.DateOfBirth.Date;
        Password = request.Password;
    }
}