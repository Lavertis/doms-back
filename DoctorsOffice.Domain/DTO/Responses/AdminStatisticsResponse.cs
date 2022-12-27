namespace DoctorsOffice.Domain.DTO.Responses;

public class AdminStatisticsResponse
{
    public int AppointmentsCount { get; set; }
    public int PrescriptionsCount { get; set; }
    public Dictionary<string, int> AppointmentTypesCount { get; set; } = new();
    public int DoctorsPatientCount { get; set; }
    public int PatientCount { get; set; }
    public int DoctorCount { get; set; }
    public int UserCount { get; set; }
}