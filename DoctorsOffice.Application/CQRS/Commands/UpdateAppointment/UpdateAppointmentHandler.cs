using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Exceptions;
using DoctorsOffice.Domain.Repositories;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.UpdateAppointment;

public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, AppointmentResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;

    public UpdateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusRepository appointmentStatusRepository,
        IAppointmentTypeRepository appointmentTypeRepository)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentStatusRepository = appointmentStatusRepository;
        _appointmentTypeRepository = appointmentTypeRepository;
    }

    public async Task<AppointmentResponse> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointmentToUpdate = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId,
            a => a.Status,
            a => a.Type);

        switch (request.Role)
        {
            case RoleTypes.Doctor when appointmentToUpdate.DoctorId != request.UserId:
                throw new ForbiddenException("Trying to update appointment of another doctor");
            case RoleTypes.Patient when appointmentToUpdate.PatientId != request.UserId:
                throw new ForbiddenException("Trying to update appointment of another patient");
        }

        if (!string.IsNullOrEmpty(request.Status) &&
            request.Role == RoleTypes.Doctor &&
            !AppointmentStatuses.AllowedTransitions[appointmentToUpdate.Status.Name].Contains(request.Status))
        {
            throw new BadRequestException(
                $"Status change from {appointmentToUpdate.Status.Name} to {request.Status} is not allowed"
            );
        }

        appointmentToUpdate.Date = request.Date ?? appointmentToUpdate.Date;
        appointmentToUpdate.Description = request.Description ?? appointmentToUpdate.Description;
        try
        {
            if (request.Type is not null)
                appointmentToUpdate.Type = await _appointmentTypeRepository.GetByNameAsync(request.Type);
            if (request.Status is not null)
                appointmentToUpdate.Status = await _appointmentStatusRepository.GetByNameAsync(request.Status);
        }
        catch (NotFoundException e)
        {
            throw new BadRequestException(e.Message);
        }

        var appointmentEntity =
            await _appointmentRepository.UpdateByIdAsync(request.AppointmentId, appointmentToUpdate);
        return new AppointmentResponse(appointmentEntity);
    }
}