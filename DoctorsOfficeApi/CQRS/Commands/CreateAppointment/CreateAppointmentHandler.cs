using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AppointmentRepository;
using DoctorsOfficeApi.Repositories.AppointmentStatusRepository;
using DoctorsOfficeApi.Repositories.AppointmentTypeRepository;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using DoctorsOfficeApi.Repositories.PatientRepository;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository,
        IAppointmentStatusRepository appointmentStatusRepository,
        IAppointmentTypeRepository appointmentTypeRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _appointmentStatusRepository = appointmentStatusRepository;
        _appointmentTypeRepository = appointmentTypeRepository;
    }

    public async Task<AppointmentResponse> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        switch (request.Role)
        {
            case RoleTypes.Doctor when request.DoctorId != request.UserId:
                throw new BadRequestException("Cannot create appointment for another doctor");
            case RoleTypes.Patient when request.PatientId != request.UserId:
                throw new BadRequestException("Cannot create appointment request for another patient");
        }

        Appointment newAppointment;
        try
        {
            newAppointment = new Appointment
            {
                Date = request.Date,
                Description = request.Description,
                PatientId = (await _patientRepository.GetByIdAsync(request.PatientId)).Id,
                DoctorId = (await _doctorRepository.GetByIdAsync(request.DoctorId)).Id,
                StatusId = (await _appointmentStatusRepository.GetByNameAsync(request.Status)).Id,
                TypeId = (await _appointmentTypeRepository.GetByNameAsync(request.Type)).Id,
            };
        }
        catch (NotFoundException e)
        {
            throw new BadRequestException(e.Message);
        }

        var createdAppointmentId = (await _appointmentRepository.CreateAsync(newAppointment)).Id;
        var createdAppointment = await _appointmentRepository.GetByIdAsync(
            createdAppointmentId,
            a => a.Status,
            a => a.Type);
        return new AppointmentResponse(createdAppointment);
    }
}