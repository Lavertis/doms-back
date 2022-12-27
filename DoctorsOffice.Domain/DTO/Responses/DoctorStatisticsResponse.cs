namespace DoctorsOffice.Domain.DTO.Responses;

public class DoctorStatisticsResponse
{
    public int AppointmentsCount { get; set; }
    public int PrescriptionsCount { get; set; }
    public Dictionary<string, int> AppointmentTypesCount { get; set; } = new();
    public int DoctorsPatientCount { get; set; }
}