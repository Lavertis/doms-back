using DoctorsOffice.SendGrid.Config;
using DoctorsOffice.SendGrid.DTO;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DoctorsOffice.SendGrid.Service;

public class SendGridService : ISendGridService
{
    private readonly SendGridClient _sendGridClient;
    private readonly SendGridSettings _sendGridSettings;

    public SendGridService(IOptions<SendGridSettings> sendGridSettings)
    {
        _sendGridSettings = sendGridSettings.Value;
        _sendGridClient = new SendGridClient(_sendGridSettings.ApiKey);
    }

    public async Task<Response> SendSingleEmailAsync(SingleEmailDto singleEmailDto)
    {
        var message = new SendGridMessage();
        message.SetFrom(_sendGridSettings.SenderEmail, _sendGridSettings.SenderName);
        message.AddTo(singleEmailDto.RecipientEmail, singleEmailDto.RecipientName);
        message.SetTemplateId(singleEmailDto.TemplateId);
        message.SetTemplateData(singleEmailDto.TemplateData);
        var response = await _sendGridClient.SendEmailAsync(message);
        return response;
    }
}