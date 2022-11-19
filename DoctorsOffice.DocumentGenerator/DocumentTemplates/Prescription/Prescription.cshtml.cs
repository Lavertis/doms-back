namespace DoctorsOffice.DocumentGenerator.DocumentTemplates.Prescription;

public class PrescriptionTemplateData
{
    public DateTime CreationDate { get; set; }
    public DateTime FulfillmentDeadline { get; set; }
    public string DoctorFullName { get; set; } = null!;
    public string PatientFullName { get; set; } = null!;
    public string PatientNationalId { get; set; } = null!;
    public List<PrescriptionTemplateDrugItem> DrugItems { get; set; } = new();
}

public class PrescriptionTemplateDrugItem
{
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!;
}