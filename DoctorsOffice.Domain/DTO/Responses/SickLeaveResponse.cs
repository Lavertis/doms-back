namespace DoctorsOffice.Domain.DTO.Responses;

public class SickLeaveResponse
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Guid DoctorId { get; set; }

    public Guid? AppointmentId { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string Diagnosis { get; set; } = "";

    public string Purpose { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}