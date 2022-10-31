namespace DoctorsOffice.SendGrid.DTO.TemplateData;

public class AppointmentReminderEmailData
{
    public string DoctorName { get; set; } = null!;
    public string Date { get; set; } = null!;
    public string WebsiteAddress { get; set; } = null!;
}