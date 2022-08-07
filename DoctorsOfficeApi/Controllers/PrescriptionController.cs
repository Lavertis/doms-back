using DoctorsOfficeApi.CQRS.Commands.CreatePrescription;
using DoctorsOfficeApi.CQRS.Commands.UpdatePrescription;
using DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByDoctorId;
using DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByPatientId;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/prescription")]
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
    public async Task<ActionResult<IList<PrescriptionResponse>>> GetPrescriptionsByPatientIdAsync(Guid patientId)
        => Ok(await Mediator.Send(new GetPrescriptionsByPatientIdQuery(patientId: patientId)));

    /// <summary>
    /// Returns all prescriptions for authenticated patient. Only for patients.
    /// </summary>
    [HttpGet("patient/auth")]
    [Authorize(Roles = RoleTypes.Patient)]
    public async Task<ActionResult<IList<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedPatientAsync()
        => Ok(await Mediator.Send(new GetPrescriptionsByPatientIdQuery(patientId: JwtSubject())));

    /// <summary>
    /// Returns all prescriptions for authenticated doctor. Only for doctors.
    /// </summary>
    [HttpGet("doctor/auth")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<IList<PrescriptionResponse>>> GetPrescriptionsForAuthenticatedDoctorAsync()
        => Ok(await Mediator.Send(new GetPrescriptionsByDoctorIdQuery(doctorId: JwtSubject())));

    /// <summary>
    /// Creates new prescription. Only for doctors.
    /// </summary>
    [HttpPost("")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> CreatePrescriptionAsync(CreatePrescriptionRequest request)
        => StatusCode(
            StatusCodes.Status201Created,
            await Mediator.Send(new CreatePrescriptionCommand(request: request, doctorId: JwtSubject()))
        );

    /// <summary>
    /// Updates prescription by id prescription. Only for doctors.
    /// </summary>
    [HttpPatch("{prescriptionId:guid}")]
    [Authorize(Roles = RoleTypes.Doctor)]
    public async Task<ActionResult<PrescriptionResponse>> UpdatePrescriptionByIdAsync(
        UpdatePrescriptionRequest request, Guid prescriptionId)
        => Ok(await Mediator.Send(new UpdatePrescriptionCommand(request: request, prescriptionId: prescriptionId)));
}