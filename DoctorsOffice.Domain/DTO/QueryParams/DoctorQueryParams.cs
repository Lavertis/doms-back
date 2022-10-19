namespace DoctorsOffice.Domain.DTO.QueryParams;

public class DoctorQueryParams
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? AccountCreationDateBegin { get; set; }
    public DateTime? AccountCreationDateEnd { get; set; }
}