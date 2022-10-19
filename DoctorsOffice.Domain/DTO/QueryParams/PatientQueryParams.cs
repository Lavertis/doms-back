namespace DoctorsOffice.Domain.DTO.QueryParams;

public class PatientQueryParams
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NationalId { get; set; }
    public DateTime? AccountCreationDateBegin { get; set; }
    public DateTime? AccountCreationDateEnd { get; set; }
}