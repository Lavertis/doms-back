using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.SickLeaves.CreateSickLeave;

public class CreateSickLeaveHandler : IRequestHandler<CreateSickLeaveCommand, HttpResult<SickLeaveResponse>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;
    private readonly IPatientRepository _patientRepository;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public CreateSickLeaveHandler(
        ISickLeaveRepository sickLeaveRepository,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository,
        IMapper mapper)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<SickLeaveResponse>> Handle(CreateSickLeaveCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<SickLeaveResponse>();

        var patient = await _patientRepository.GetByIdAsync(request.PatientId);
        if (patient is null)
        {
            return result
                .WithError(new Error {Message = $"Patient with id {request.PatientId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        if (doctor is null)
        {
            return result
                .WithError(new Error {Message = $"Doctor with id {request.DoctorId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var newSickLeave = new SickLeave
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            AppointmentId = request.AppointmentId,
            DateStart = request.DateStart,
            DateEnd = request.DateEnd,
            Diagnosis = request.Diagnosis,
            Purpose = request.Purpose
        };
        var createdSickLeave = await _sickLeaveRepository.CreateAsync(newSickLeave);
        var sickLeaveResponse = _mapper.Map<SickLeaveResponse>(createdSickLeave);
        return result
            .WithValue(sickLeaveResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }
}