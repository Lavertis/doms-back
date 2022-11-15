namespace DoctorsOffice.Infrastructure.Config;

public class QuickButtonSettings
{
    public string[] DefaultInterviewQuickButtons { get; set; } = Array.Empty<string>();
    public string[] DefaultDiagnosisQuickButtons { get; set; } = Array.Empty<string>();
    public string[] DefaultRecommendationsQuickButtons { get; set; } = Array.Empty<string>();
}