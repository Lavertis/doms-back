namespace DoctorsOffice.Domain.DTO.Responses;

public class QuickButtonResponse
{
    public Guid Id { get; set; }
    public string Value { get; set; } = null!;
    public string Type { get; set; } = null!;
}