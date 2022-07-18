using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
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
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;

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