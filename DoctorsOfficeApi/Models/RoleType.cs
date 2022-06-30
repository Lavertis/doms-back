namespace DoctorsOfficeApi.Models;

public readonly record struct RoleType
{
    public const string Admin = "Admin";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
}