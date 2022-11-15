namespace DoctorsOffice.Domain.Enums;

public readonly record struct QuickButtonTypes
{
    public const string Interview = nameof(Interview);
    public const string Diagnosis = nameof(Diagnosis);
    public const string Recommendations = nameof(Recommendations);
}