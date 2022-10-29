using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
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

namespace DoctorsOffice.Application.CQRS.Commands.Users.PasswordReset;

public class PasswordResetHandler : IRequestHandler<PasswordResetCommand, HttpResult<PasswordResetResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IdentitySettings _identitySettings;
    private readonly ISendGridService _sendGridService;
    private readonly SendGridTemplateSettings _sendGridTemplateSettings;
    private readonly UrlSettings _urlSettings;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PasswordResetHandler(
        AppUserManager appUserManager,
        ISendGridService sendGridService,
        IOptions<IdentitySettings> identitySettings,
        IOptions<SendGridTemplateSettings> sendGridTemplateSettings,
        IOptions<UrlSettings> urlSettings,
        IWebHostEnvironment webHostEnvironment)
    {
        _appUserManager = appUserManager;
        _sendGridService = sendGridService;
        _urlSettings = urlSettings.Value;
        _webHostEnvironment = webHostEnvironment;
        _sendGridTemplateSettings = sendGridTemplateSettings.Value;
        _identitySettings = identitySettings.Value;
    }

    public async Task<HttpResult<PasswordResetResponse>> Handle(PasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<PasswordResetResponse>();
        var data = request.Data;
        var findUserResult = await _appUserManager.FindByEmailAsync(data.Email);
        if (findUserResult.IsError || findUserResult.Value == null)
        {
            return result
                .WithStatusCode(StatusCodes.Status404NotFound)
                .WithError(findUserResult.Error);
        }

        var appUser = findUserResult.Value;
        var passwordResetToken = await _appUserManager.GeneratePasswordResetTokenAsync(appUser);
        var uriBuilder = new UriBuilder(_urlSettings.FrontendDomain)
        {
            Path = "password-reset/new",
            Query = $"email={appUser.Email}&token={Uri.EscapeDataString(passwordResetToken)}"
        };
        var passwordResetLink = uriBuilder.Uri.ToString();

        if (_webHostEnvironment.EnvironmentName != "Development")
        {
            await SendPasswordResetEmailAsync(appUser, passwordResetLink);
        }

        var passwordResetResponse = new PasswordResetResponse
        {
            Token = passwordResetToken,
            PasswordResetLink = passwordResetLink
        };
        return result.WithValue(passwordResetResponse);
    }

    private async Task SendPasswordResetEmailAsync(AppUser appUser, string passwordResetLink)
    {
        var email = new SingleEmailDto
        {
            RecipientEmail = appUser.Email,
            RecipientName = appUser.FirstName,
            TemplateId = _sendGridTemplateSettings.PasswordReset,
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