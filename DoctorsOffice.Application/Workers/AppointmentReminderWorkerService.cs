using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.SendGrid.DTO;
using DoctorsOffice.SendGrid.DTO.TemplateData;
using DoctorsOffice.SendGrid.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Application.Workers;

public class AppointmentReminderWorkerService : BackgroundService
{
    private readonly ILogger<AppointmentReminderWorkerService> _logger;
    private readonly TimeSpan _period;
    private readonly SendGridTemplateSettings _sendGridTemplateSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly UrlSettings _urlSettings;
    private int _executionCount;

    public AppointmentReminderWorkerService(
        ILogger<AppointmentReminderWorkerService> logger,
        IServiceProvider serviceProvider,
        IOptions<SendGridTemplateSettings> sendGridTemplateSettings,
        IOptions<UrlSettings> urlSettings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _urlSettings = urlSettings.Value;
        _sendGridTemplateSettings = sendGridTemplateSettings.Value;
        _period = TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker has started");
        var timeSpanTillNearestHour = TimeSpanTillNearestHour();
        await Task.Delay(timeSpanTillNearestHour, stoppingToken);
        using var timer = new PeriodicTimer(_period);

        do
        {
            await SendEmailRemindersAboutAppointmentsAsync();
            _executionCount++;
            _logger.LogInformation("Email reminders about appointments have been sent {Count} times", _executionCount);
        } while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);

        _logger.LogInformation("Worker has finished");
    }

    private async Task SendEmailRemindersAboutAppointmentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var sendGridService = scope.ServiceProvider.GetRequiredService<ISendGridService>();
        var appointmentsIn24Hours = await appointmentRepository.GetAll()
            .Include(appointment => appointment.Patient.AppUser)
            .Include(appointment => appointment.Doctor.AppUser)
            .Where(a => a.Date >= DateTime.UtcNow && a.Date <= DateTime.UtcNow.AddHours(24))
            .ToListAsync();

        foreach (var appointment in appointmentsIn24Hours)
        {
            var email = new SingleEmailDto
            {
                RecipientEmail = appointment.Patient.AppUser.Email,
                RecipientName = appointment.Patient.AppUser.FirstName,
                TemplateId = _sendGridTemplateSettings.AppointmentReminder,
                TemplateData = new AppointmentReminderEmailData
                {
                    DoctorName = appointment.Doctor.AppUser.FirstName,
                    WebsiteAddress = _urlSettings.FrontendDomain,
                    Date = $"{appointment.Date.ToShortDateString()} {appointment.Date.ToShortTimeString()}"
                }
            };
            // await sendGridService.SendSingleEmailAsync(email);
        }
    }

    private static TimeSpan TimeSpanTillNearestHour()
    {
        return TimeSpan.FromMinutes(60 - DateTime.UtcNow.Minute);
    }
}