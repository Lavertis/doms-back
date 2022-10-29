using DoctorsOffice.SendGrid.Config;
using DoctorsOffice.SendGrid.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.SendGrid;

public static class SendGridModule
{
    public static void AddSendGrid(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));
        services.AddSingleton<ISendGridService, SendGridService>();
    }
}