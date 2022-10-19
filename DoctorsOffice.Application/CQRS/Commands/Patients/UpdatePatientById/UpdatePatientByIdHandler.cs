using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.UpdatePatientById;

public class UpdatePatientByIdHandler : IRequestHandler<UpdatePatientByIdCommand, HttpResult<PatientResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IMapper _mapper;
    private readonly IPatientRepository _patientRepository;

    public UpdatePatientByIdHandler(IPatientRepository patientRepository, AppUserManager appUserManager, IMapper mapper)
    {
        _patientRepository = patientRepository;
        _appUserManager = appUserManager;
        _mapper = mapper;
    }

    public async Task<HttpResult<PatientResponse>> Handle(UpdatePatientByIdCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<PatientResponse>();

        var patient = await _patientRepository.GetByIdAsync(request.PatientId);
        if (patient is null)
        {
            return result
                .WithError(new Error {Message = $"Patient with id {request.PatientId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var appUser = await _appUserManager.Users.FirstAsync(x => x.Id == patient.Id, cancellationToken);

        appUser.UserName = request.UserName ?? appUser.UserName;
        appUser.Email = request.Email ?? appUser.Email;
        appUser.PhoneNumber = request.PhoneNumber ?? appUser.PhoneNumber;
        appUser.FirstName = request.FirstName ?? appUser.FirstName;
        appUser.LastName = request.LastName ?? appUser.LastName;
        if (!string.IsNullOrEmpty(request.NewPassword))
            appUser.PasswordHash = _appUserManager.PasswordHasher.HashPassword(appUser, request.NewPassword);

        patient.NationalId = request.NationalId ?? patient.NationalId;
        patient.Address = request.Address ?? patient.Address;
        patient.DateOfBirth = request.DateOfBirth ?? patient.DateOfBirth;

        await _appUserManager.UpdateAsync(appUser);
        await _patientRepository.UpdateAsync(patient);
        patient.AppUser = appUser;

        var patientResponse = _mapper.Map<PatientResponse>(patient);
        return result.WithValue(patientResponse);
    }
}