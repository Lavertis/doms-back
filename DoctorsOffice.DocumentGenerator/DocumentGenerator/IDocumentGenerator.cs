using DoctorsOffice.DocumentGenerator.DocumentTemplates.Prescription;
using DoctorsOffice.DocumentGenerator.DocumentTemplates.SickLeave;

namespace DoctorsOffice.DocumentGenerator.DocumentGenerator;

public interface IDocumentGenerator
{
    Task<MemoryStream> GeneratePrescriptionAsPdf(PrescriptionTemplateData data);
    Task<MemoryStream> GenerateSickLeaveAsPdf(SickLeaveTemplateData data);
}