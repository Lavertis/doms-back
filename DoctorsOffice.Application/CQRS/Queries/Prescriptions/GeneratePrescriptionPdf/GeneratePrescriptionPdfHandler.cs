using DoctorsOffice.DocumentGenerator.DocumentGenerator;
using DoctorsOffice.DocumentGenerator.DocumentTemplates.Prescription;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GeneratePrescriptionPdf;

public class GeneratePrescriptionPdfHandler : IRequestHandler<GeneratePrescriptionPdfQuery, HttpResult<FileResult>>
{
    private readonly IDocumentGenerator _documentGenerator;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GeneratePrescriptionPdfHandler(
        IDocumentGenerator documentGenerator,
        IPrescriptionRepository prescriptionRepository)
    {
        _documentGenerator = documentGenerator;
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<HttpResult<FileResult>> Handle(GeneratePrescriptionPdfQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<FileResult>();
        var prescription = await _prescriptionRepository.GetAll()
            .Include(prescription => prescription.Doctor.AppUser)
            .Include(prescription => prescription.Patient.AppUser)
            .Include(prescription => prescription.DrugItems)
            .FirstOrDefaultAsync(prescription => prescription.Id == request.PrescriptionId, cancellationToken);

        if (prescription == null)
        {
            return result
                .WithError(new Error {Message = $"Prescription with id {request.PrescriptionId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        if (request.Role is Roles.Patient && prescription.Patient.AppUser.Id != request.AppUserId)
        {
            return result
                .WithError(new Error {Message = "You are not allowed to view this prescription"})
                .WithStatusCode(StatusCodes.Status403Forbidden);
        }

        var doctorFullName = $"{prescription.Doctor.AppUser.FirstName} {prescription.Doctor.AppUser.LastName}";
        var patientFullName = $"{prescription.Patient.AppUser.FirstName} {prescription.Patient.AppUser.LastName}";
        var templateData = new PrescriptionTemplateData
        {
            CreationDate = prescription.CreatedAt,
            FulfillmentDeadline = prescription.FulfillmentDeadline,
            DoctorFullName = doctorFullName,
            PatientFullName = patientFullName,
            PatientNationalId = prescription.Patient.NationalId,
            DrugItems = prescription.DrugItems.Select(drugItem => new PrescriptionTemplateDrugItem
            {
                Name = drugItem.Name,
                Dosage = drugItem.Dosage,
                Quantity = drugItem.Quantity
            }).ToList()
        };
        var pdfStream = await _documentGenerator.GeneratePrescriptionAsPdf(templateData);
        var fileName = $"Prescription - {DateTime.Now:yyyy-MM-dd HH:mm}.pdf";
        var fileResult = new FileResult
        {
            Stream = pdfStream,
            ContentType = ContentTypes.ApplicationPdf,
            FileName = fileName
        };
        return result.WithValue(fileResult);
    }
}