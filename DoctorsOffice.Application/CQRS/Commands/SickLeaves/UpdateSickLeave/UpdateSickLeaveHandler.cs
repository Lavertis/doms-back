using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.SickLeaves.UpdateSickLeave;

public class UpdateSickLeaveHandler : IRequestHandler<UpdateSickLeaveCommand, HttpResult<SickLeaveResponse>>
{
    private readonly IMapper _mapper;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public UpdateSickLeaveHandler(
        ISickLeaveRepository sickLeaveRepository,
        IMapper mapper)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<SickLeaveResponse>> Handle(UpdateSickLeaveCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<SickLeaveResponse>();

        var sickLeaveToUpdate = await _sickLeaveRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.SickLeaveId, cancellationToken);

        if (sickLeaveToUpdate is null)
        {
            return result
                .WithError(new Error {Message = $"Sick leave with id {request.SickLeaveId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        if (request.DateStart >= sickLeaveToUpdate.DateEnd)
        {
            return result
                .WithError(new Error {Message = $"Date start must before {sickLeaveToUpdate.DateEnd}"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        if (request.DateEnd <= sickLeaveToUpdate.DateStart)
        {
            return result
                .WithError(new Error {Message = $"Date end must after {sickLeaveToUpdate.DateStart}"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        sickLeaveToUpdate.Diagnosis = request.Diagnosis ?? sickLeaveToUpdate.Diagnosis;
        sickLeaveToUpdate.Purpose = request.Purpose ?? sickLeaveToUpdate.Purpose;
        sickLeaveToUpdate.PatientId = request.PatientId ?? sickLeaveToUpdate.PatientId;
        sickLeaveToUpdate.DoctorId = request.DoctorId ?? sickLeaveToUpdate.DoctorId;
        sickLeaveToUpdate.AppointmentId = request.AppointmentId ?? sickLeaveToUpdate.AppointmentId;
        sickLeaveToUpdate.DateStart = request.DateStart ?? sickLeaveToUpdate.DateStart;
        sickLeaveToUpdate.DateEnd = request.DateEnd ?? sickLeaveToUpdate.DateEnd;

        var sickLeaveEntity = await _sickLeaveRepository.UpdateAsync(sickLeaveToUpdate);
        var sickLeaveResponse = _mapper.Map<SickLeaveResponse>(sickLeaveEntity);
        return result.WithValue(sickLeaveResponse);
    }
}