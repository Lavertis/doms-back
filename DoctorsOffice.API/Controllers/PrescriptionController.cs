using DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;
using DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByDoctorId;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByPatientId;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/prescriptions")]
public class PrescriptionController : BaseController
{
    public PrescriptionController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all prescriptions for specified patient. Only for doctors.
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetPrescriptionsByPatientIdAsync(Guid patientId)
        => CreateResponse(await Mediator.Send(new GetPrescriptionsByPatientIdQuery(patientId)));

    /// <summary>
    /// Returns all prescriptions for authenticated patient. Only for patients.
    /// </summary>
    [HttpGet("patient/current")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedPatientAsync()
        => CreateResponse(await Mediator.Send(new GetPrescriptionsByPatientIdQuery(JwtSubject())));

    /// <summary>
    /// Returns all prescriptions for authenticated doctor. Only for doctors.
    /// </summary>
    [HttpGet("doctor/current")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedDoctorAsync()
        => CreateResponse(await Mediator.Send(new GetPrescriptionsByDoctorIdQuery(JwtSubject())));

    /// <summary>
    /// Creates new prescription. Only for doctors.
    /// </summary>
    [HttpPost("doctor/current")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> CreatePrescriptionAsync(CreatePrescriptionRequest request)
        => CreateResponse(await Mediator.Send(new CreatePrescriptionCommand(request, JwtSubject())));

    /// <summary>
    /// Updates prescription by id prescription. Only for doctors.
    /// </summary>
    [HttpPatch("{prescriptionId:guid}")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> UpdatePrescriptionByIdAsync(
        UpdatePrescriptionRequest request, Guid prescriptionId)
        => CreateResponse(
            await Mediator.Send(new UpdatePrescriptionCommand(request, prescriptionId))
        );
}