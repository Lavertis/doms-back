namespace DoctorsOffice.Domain.DTO.Responses;

public class DoctorQuickButtonsResponse
{
    public List<QuickButtonResponse> InterviewQuickButtons { get; set; } = new();
    public List<QuickButtonResponse> DiagnosisQuickButtons { get; set; } = new();
    public List<QuickButtonResponse> RecommendationsQuickButtons { get; set; } = new();
}