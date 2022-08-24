using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.Appointments.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, HttpResult<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;
    private readonly IPatientRepository _patientRepository;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository,
        IAppointmentStatusRepository appointmentStatusRepository,
        IAppointmentTypeRepository appointmentTypeRepository,
        IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _appointmentStatusRepository = appointmentStatusRepository;
        _appointmentTypeRepository = appointmentTypeRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<AppointmentResponse>> Handle(
        CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AppointmentResponse>();

        switch (request.Role)
        {
            case RoleTypes.Doctor when request.DoctorId != request.UserId:
                return result
                    .WithError(new Error {Message = "Cannot create appointment for another doctor"})
                    .WithStatusCode(StatusCodes.Status403Forbidden);
            case RoleTypes.Patient when request.PatientId != request.UserId:
                return result
                    .WithError(new Error {Message = "Cannot create appointment request for another patient"})
                    .WithStatusCode(StatusCodes.Status403Forbidden);
        }

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

        var appointmentStatus = await _appointmentStatusRepository.GetByNameAsync(request.Status);
        if (appointmentStatus is null)
        {
            return result
                .WithError(new Error {Message = $"AppointmentStatus with name {request.Status} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var appointmentType = await _appointmentTypeRepository.GetByNameAsync(request.Type);
        if (appointmentType is null)
        {
            return result
                .WithError(new Error {Message = $"AppointmentType with name {request.Type} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var newAppointment = new Appointment
        {
            Date = request.Date,
            Description = request.Description,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            StatusId = appointmentStatus.Id,
            TypeId = appointmentType.Id,
        };

        var createdAppointmentId = (await _appointmentRepository.CreateAsync(newAppointment)).Id;
        var createdAppointment = await _appointmentRepository.GetAll()
            .Include(appointment => appointment.Status)
            .Include(appointment => appointment.Type)
            .FirstOrDefaultAsync(appointment => appointment.Id == createdAppointmentId, cancellationToken);

        var appointmentResponse = _mapper.Map<AppointmentResponse>(createdAppointment);
        return result
            .WithValue(appointmentResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }
}