namespace DoctorsOffice.Domain.DTO.Responses;

public class PrescriptionResponse
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<DrugItemResponse> DrugItems { get; set; } = null!;
}