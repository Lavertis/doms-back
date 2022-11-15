using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.Entities;

public class QuickButton : BaseEntity
{
    public string Value { get; set; } = null!;
    public string Type { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
}