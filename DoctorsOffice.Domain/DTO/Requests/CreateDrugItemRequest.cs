namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateDrugItemRequest
{
    public long Rxcui { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!;
}