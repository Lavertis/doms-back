namespace DoctorsOffice.SendGrid.DTO.TemplateData;

public class AppointmentStatusUpdateTemplateData
{
    public string DoctorName { get; set; } = null!;
    public string Date { get; set; } = null!;
    public string PreviousStatus { get; set; } = null!;
    public string CurrentStatus { get; set; } = null!;
    public string WebsiteAddress { get; set; } = null!;
}