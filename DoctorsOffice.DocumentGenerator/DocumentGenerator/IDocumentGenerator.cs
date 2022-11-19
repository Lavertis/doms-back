using DoctorsOffice.DocumentGenerator.DocumentTemplates.Prescription;

namespace DoctorsOffice.DocumentGenerator.DocumentGenerator;

public interface IDocumentGenerator
{
    Task<MemoryStream> GeneratePrescriptionAsPdf(PrescriptionTemplateData data);
}