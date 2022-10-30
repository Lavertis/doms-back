using AutoMapper;
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

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;

public class CreateDoctorHandler : IRequestHandler<CreateDoctorCommand, HttpResult<CreateDoctorResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IdentitySettings _identitySettings;
    private readonly IMapper _mapper;
    private readonly ISendGridService _sendGridService;
    private readonly SendGridTemplateSettings _sendGridTemplateSettings;
    private readonly UrlSettings _urlSettings;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreateDoctorHandler(
        IDoctorRepository doctorRepository,
        IUserService userService,
        IMapper mapper,
        ISendGridService sendGridService,
        IOptions<SendGridTemplateSettings> sendGridTemplateSettings,
        IOptions<IdentitySettings> identitySettings,
        IWebHostEnvironment webHostEnvironment,
        AppUserManager appUserManager,
        IOptions<UrlSettings> urlSettings)
    {
        _doctorRepository = doctorRepository;
        _userService = userService;
        _mapper = mapper;
        _sendGridService = sendGridService;
        _webHostEnvironment = webHostEnvironment;
        _appUserManager = appUserManager;
        _urlSettings = urlSettings.Value;
        _identitySettings = identitySettings.Value;
        _sendGridTemplateSettings = sendGridTemplateSettings.Value;
    }

    public async Task<HttpResult<CreateDoctorResponse>> Handle(CreateDoctorCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<CreateDoctorResponse>();

        var createUserResult = await _userService.CreateUserAsync(new CreateUserRequest
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Password = Guid.NewGuid().ToString(),
            RoleName = Roles.Doctor,
            EmailConfirmed = true
        });
        if (createUserResult.IsError || createUserResult.HasValidationErrors)
        {
            if (createUserResult.HasValidationErrors)
                return result.WithValidationErrors(createUserResult.ValidationErrors);
            return result.WithError(createUserResult.Error).WithStatusCode(createUserResult.StatusCode);
        }

        var newAppUser = createUserResult.Value!;
        var newDoctor = new Doctor {AppUser = newAppUser};
        var doctorEntity = await _doctorRepository.CreateAsync(newDoctor);
        var doctorResponse = _mapper.Map<DoctorResponse>(doctorEntity);

        var passwordResetToken = await _appUserManager.GeneratePasswordResetTokenAsync(newAppUser);
        var uriBuilder = new UriBuilder(_urlSettings.FrontendDomain)
        {
            Path = "password-reset/new",
            Query = $"email={newAppUser.Email}&token={Uri.EscapeDataString(passwordResetToken)}"
        };
        var passwordResetLink = uriBuilder.Uri.ToString();
        if (_webHostEnvironment.EnvironmentName != "Development")
        {
            await SendDoctorPasswordSetupEmailAsync(newAppUser, passwordResetLink);
        }

        var createDoctorResponse = new CreateDoctorResponse
        {
            Doctor = doctorResponse,
            PasswordResetLink = passwordResetLink,
            PasswordResetToken = passwordResetToken
        };
        return result
            .WithValue(createDoctorResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }

    private async Task SendDoctorPasswordSetupEmailAsync(AppUser appUser, string passwordResetLink)
    {
        var email = new SingleEmailDto
        {
            RecipientEmail = appUser.Email,
            RecipientName = appUser.FirstName,
            TemplateId = _sendGridTemplateSettings.DoctorPasswordSetup,
            TemplateData = new PasswordResetEmailData
            {
                FirstName = appUser.FirstName,
                ExpirationTimeInHours = _identitySettings.PasswordResetTokenLifeSpanInHours,
                PasswordResetLink = passwordResetLink
            }
        };
        await _sendGridService.SendSingleEmailAsync(email);
    }
}