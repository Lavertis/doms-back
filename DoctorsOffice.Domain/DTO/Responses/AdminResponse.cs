namespace DoctorsOffice.Domain.DTO.Responses;

public class AdminResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
}