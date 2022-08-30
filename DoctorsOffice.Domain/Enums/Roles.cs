namespace DoctorsOffice.Domain.Enums;

public readonly record struct Roles
{
    public const string Admin = nameof(Admin);
    public const string Patient = nameof(Patient);
    public const string Doctor = nameof(Doctor);
}