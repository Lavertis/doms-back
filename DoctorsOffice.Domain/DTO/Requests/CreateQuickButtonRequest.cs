namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateQuickButtonRequest
{
    public string Value { get; set; } = null!;
    public string Type { get; set; } = null!;
}