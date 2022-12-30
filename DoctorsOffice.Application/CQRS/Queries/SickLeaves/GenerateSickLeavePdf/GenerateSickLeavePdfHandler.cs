using DoctorsOffice.DocumentGenerator.DocumentGenerator;
using DoctorsOffice.DocumentGenerator.DocumentTemplates.SickLeave;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GenerateSickLeavePdf;

public class GenerateSickLeavePdfHandler : IRequestHandler<GenerateSickLeavePdfQuery, HttpResult<FileResult>>
{
    private readonly IDocumentGenerator _documentGenerator;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public GenerateSickLeavePdfHandler(IDocumentGenerator documentGenerator, ISickLeaveRepository sickLeaveRepository)
    {
        _documentGenerator = documentGenerator;
        _sickLeaveRepository = sickLeaveRepository;
    }

    public async Task<HttpResult<FileResult>> Handle(GenerateSickLeavePdfQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<FileResult>();
        var sickLeave = await _sickLeaveRepository.GetAll()
            .Include(sickLeave => sickLeave.Doctor.AppUser)
            .Include(sickLeave => sickLeave.Patient.AppUser)
            .FirstOrDefaultAsync(sickLeave => sickLeave.Id == request.SickLeaveId, cancellationToken);

        if (sickLeave == null)
        {
            return result
                .WithError(new Error {Message = $"Sick leave with id {request.SickLeaveId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        if (request.Role is Roles.Patient && sickLeave.Patient.AppUser.Id != request.AppUserId)
        {
            return result
                .WithError(new Error {Message = "You are not allowed to view this sick leave"})
                .WithStatusCode(StatusCodes.Status403Forbidden);
        }

        var doctorFullName = $"{sickLeave.Doctor.AppUser.FirstName} {sickLeave.Doctor.AppUser.LastName}";
        var patientFullName = $"{sickLeave.Patient.AppUser.FirstName} {sickLeave.Patient.AppUser.LastName}";
        var templateData = new SickLeaveTemplateData
        {
            CreatedAt = sickLeave.CreatedAt,
            DoctorFullName = doctorFullName,
            PatientFullName = patientFullName,
            PatientNationalId = sickLeave.Patient.NationalId,
            StartDate = sickLeave.DateStart,
            EndDate = sickLeave.DateEnd,
            Diagnosis = sickLeave.Diagnosis,
            Purpose = sickLeave.Purpose
        };
        var pdfStream = await _documentGenerator.GenerateSickLeaveAsPdf(templateData);
        var fileName = $"Sick leave - {DateTime.Now:yyyy-MM-dd HH:mm}.pdf";
        var fileResult = new FileResult
        {
            Stream = pdfStream,
            ContentType = ContentTypes.ApplicationPdf,
            FileName = fileName
        };
        return result.WithValue(fileResult);
    }
}