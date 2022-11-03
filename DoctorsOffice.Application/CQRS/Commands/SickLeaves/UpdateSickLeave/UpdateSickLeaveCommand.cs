using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.SickLeaves.UpdateSickLeave;

public class UpdateSickLeaveCommand : IRequest<HttpResult<SickLeaveResponse>>
{
    public readonly Guid? PatientId;
    public readonly Guid? DoctorId;
    public readonly Guid? AppointmentId;
    public readonly DateTime? DateStart;
    public readonly DateTime? DateEnd;
    public readonly string? Diagnosis;
    public readonly string? Purpose;

    public UpdateSickLeaveCommand(UpdateSickLeaveRequest request)
    {
        PatientId = request.PatientId;
        DoctorId = request.DoctorId;
        AppointmentId = request.AppointmentId;
        DateStart = request.DateStart;
        DateEnd = request.DateEnd;
        Diagnosis = request.Diagnosis;
        Purpose = request.Purpose;
    }
    
    public Guid SickLeaveId { get; set; }
}