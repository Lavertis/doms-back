namespace DoctorsOffice.Domain.Enums;

public readonly record struct AppointmentTypes
{
    public const string Consultation = nameof(Consultation);
    public const string Checkup = nameof(Checkup);
}