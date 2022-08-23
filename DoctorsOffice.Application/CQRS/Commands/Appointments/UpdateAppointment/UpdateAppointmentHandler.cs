using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Appointments.UpdateAppointment;

public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, HttpResult<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;
    private readonly IMapper _mapper;

    public UpdateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusRepository appointmentStatusRepository,
        IAppointmentTypeRepository appointmentTypeRepository,
        IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentStatusRepository = appointmentStatusRepository;
        _appointmentTypeRepository = appointmentTypeRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<AppointmentResponse>> Handle(
        UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AppointmentResponse>();

        var appointmentToUpdate = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId,
            a => a.Status,
            a => a.Type);
        if (appointmentToUpdate is null)
        {
            return result
                .WithError(new Error {Message = $"Appointment with id {request.AppointmentId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        switch (request.Role)
        {
            case RoleTypes.Doctor when appointmentToUpdate.DoctorId != request.UserId:
                return result
                    .WithError(new Error {Message = "Trying to update appointment of another doctor"})
                    .WithStatusCode(StatusCodes.Status403Forbidden);
            case RoleTypes.Patient when appointmentToUpdate.PatientId != request.UserId:
                return result
                    .WithError(new Error {Message = "Trying to update appointment of another patient"})
                    .WithStatusCode(StatusCodes.Status403Forbidden);
        }

        if (!string.IsNullOrEmpty(request.Status) &&
            request.Role == RoleTypes.Doctor &&
            !AppointmentStatuses.AllowedTransitions[appointmentToUpdate.Status.Name].Contains(request.Status))
        {
            return result
                .WithError(new Error
                {
                    Message = $"Status change from {appointmentToUpdate.Status.Name} to {request.Status} is not allowed"
                })
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        appointmentToUpdate.Date = request.Date ?? appointmentToUpdate.Date;
        appointmentToUpdate.Description = request.Description ?? appointmentToUpdate.Description;
        if (request.Type is not null)
        {
            var appointmentType = await _appointmentTypeRepository.GetByNameAsync(request.Type);
            if (appointmentType is null)
            {
                return result
                    .WithError(new Error {Message = $"AppointmentType with name {request.Type} not found"})
                    .WithStatusCode(StatusCodes.Status404NotFound);
            }

            appointmentToUpdate.Type = appointmentType;
        }

        if (request.Status is not null)
        {
            var appointmentStatus = await _appointmentStatusRepository.GetByNameAsync(request.Status);
            if (appointmentStatus is null)
            {
                return result
                    .WithError(new Error {Message = $"AppointmentStatus with name {request.Status} not found"})
                    .WithStatusCode(StatusCodes.Status404NotFound);
            }

            appointmentToUpdate.Status = appointmentStatus;
        }

        var appointmentEntity =
            await _appointmentRepository.UpdateByIdAsync(request.AppointmentId, appointmentToUpdate);
        var appointmentResponse = _mapper.Map<AppointmentResponse>(appointmentEntity);
        return result.WithValue(appointmentResponse);
    }
}