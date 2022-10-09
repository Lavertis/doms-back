namespace DoctorsOffice.Domain.DTO.Responses;

public class DrugItemResponse
{
    public Guid Id { get; set; }
    public long Rxcui { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!;
}