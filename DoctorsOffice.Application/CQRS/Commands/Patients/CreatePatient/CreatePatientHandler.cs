using DoctorsOffice.Application.Services.Users;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using DoctorsOffice.SendGrid.DTO;
using DoctorsOffice.SendGrid.DTO.TemplateData;
using DoctorsOffice.SendGrid.Service;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, HttpResult<CreatePatientResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IdentitySettings _identitySettings;
    private readonly IPatientRepository _patientRepository;
    private readonly ISendGridService _sendGridService;
    private readonly SendGridTemplateSettings _sendGridTemplateSettings;
    private readonly UrlSettings _urlSettings;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreatePatientHandler(
        IPatientRepository patientRepository,
        IUserService userService,
        AppUserManager appUserManager,
        IOptions<UrlSettings> urlSettings,
        ISendGridService sendGridService,
        IOptions<SendGridTemplateSettings> sendGridTemplateSettings,
        IOptions<IdentitySettings> identitySettings,
        IWebHostEnvironment webHostEnvironment)
    {
        _patientRepository = patientRepository;
        _userService = userService;
        _appUserManager = appUserManager;
        _sendGridService = sendGridService;
        _sendGridTemplateSettings = sendGridTemplateSettings.Value;
        _identitySettings = identitySettings.Value;
        _webHostEnvironment = webHostEnvironment;
        _urlSettings = urlSettings.Value;
    }

    public async Task<HttpResult<CreatePatientResponse>> Handle(
        CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<CreatePatientResponse>();

        var createUserResult = await _userService.CreateUserAsync(new CreateUserRequest
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            RoleName = Roles.Patient
        });
        if (createUserResult.IsError || createUserResult.HasValidationErrors)
        {
            if (createUserResult.HasValidationErrors)
                return result.WithValidationErrors(createUserResult.ValidationErrors);
            return result.WithError(createUserResult.Error).WithStatusCode(createUserResult.StatusCode);
        }

        var newAppUser = createUserResult.Value!;
        var newPatient = new Patient
        {
            NationalId = request.NationalId,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            AppUser = newAppUser
        };

        await _patientRepository.CreateAsync(newPatient);
        var emailConfirmationToken = await _appUserManager.GenerateEmailConfirmationTokenAsync(newAppUser);
        var uriBuilder = new UriBuilder(_urlSettings.FrontendDomain)
        {
            Path = "confirm-email",
            Query = $"token={Uri.EscapeDataString(emailConfirmationToken)}&email={newAppUser.Email}"
        };
        var confirmationLink = uriBuilder.Uri.ToString();

        if (_webHostEnvironment.EnvironmentName != "Development")
        {
            await SendConfirmationEmailAsync(
                emailAddress: request.Email,
                recipientName: request.FirstName,
                confirmationLink: confirmationLink,
                websiteAddress: _urlSettings.FrontendDomain
            );
        }

        var createPatientResponse = new CreatePatientResponse
        {
            Email = request.Email,
            Password = request.Password,
            EmailConfirmationToken = emailConfirmationToken,
            ConfirmationLink = confirmationLink
        };

        return result
            .WithValue(createPatientResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }

    private async Task SendConfirmationEmailAsync(
        string emailAddress, string recipientName, string confirmationLink, string websiteAddress)
    {
        var email = new SingleEmailDto
        {
            RecipientEmail = emailAddress,
            RecipientName = recipientName,
            TemplateId = _sendGridTemplateSettings.EmailConfirmation,
            TemplateData = new ConfirmEmailTemplateData
            {
                ExpirationTimeInHours = _identitySettings.EmailConfirmationTokenLifeSpanInHours,
                ConfirmationLink = confirmationLink,
                FirstName = recipientName,
                WebsiteAddress = websiteAddress
            }
        };
        await _sendGridService.SendSingleEmailAsync(email);
    }
}