using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.SickLeaves.CreateSickLeave;

public class CreateSickLeaveCommand : IRequest<HttpResult<SickLeaveResponse>>
{
    public readonly Guid PatientId;
    public readonly Guid DoctorId;
    public readonly Guid? AppointmentId;
    public readonly DateTime DateStart;
    public readonly DateTime DateEnd;
    public readonly string Diagnosis;
    public readonly string Purpose;

    public CreateSickLeaveCommand(CreateSickLeaveRequest request, Guid doctorId)
    {
        PatientId = request.PatientId;
        DoctorId = doctorId;
        AppointmentId = request.AppointmentId;
        DateStart = request.DateStart;
        DateEnd = request.DateEnd;
        Diagnosis = request.Diagnosis;
        Purpose = request.Purpose;
    }
}