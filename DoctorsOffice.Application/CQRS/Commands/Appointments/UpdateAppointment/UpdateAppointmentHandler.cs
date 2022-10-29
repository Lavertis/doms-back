using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.SendGrid.DTO;
using DoctorsOffice.SendGrid.DTO.TemplateData;
using DoctorsOffice.SendGrid.Service;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Application.CQRS.Commands.Appointments.UpdateAppointment;

public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, HttpResult<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;
    private readonly IMapper _mapper;
    private readonly ISendGridService _sendGridService;
    private readonly SendGridTemplateSettings _sendGridTemplateSettings;
    private readonly UrlSettings _urlSettings;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UpdateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusRepository appointmentStatusRepository,
        IAppointmentTypeRepository appointmentTypeRepository,
        IMapper mapper,
        ISendGridService sendGridService,
        IOptions<UrlSettings> urlSettings,
        IOptions<SendGridTemplateSettings> sendGridTemplateSettings,
        IWebHostEnvironment webHostEnvironment)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentStatusRepository = appointmentStatusRepository;
        _appointmentTypeRepository = appointmentTypeRepository;
        _mapper = mapper;
        _sendGridService = sendGridService;
        _webHostEnvironment = webHostEnvironment;
        _sendGridTemplateSettings = sendGridTemplateSettings.Value;
        _urlSettings = urlSettings.Value;
    }

    public async Task<HttpResult<AppointmentResponse>> Handle(
        UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AppointmentResponse>();

        var appointmentToUpdate = await _appointmentRepository.GetAll()
            .Include(appointment => appointment.Status)
            .Include(appointment => appointment.Type)
            .Include(appointment => appointment.Patient.AppUser)
            .FirstOrDefaultAsync(appointment => appointment.Id == request.AppointmentId, cancellationToken);

        if (appointmentToUpdate is null)
        {
            return result
                .WithError(new Error {Message = $"Appointment with id {request.AppointmentId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        switch (request.RoleName)
        {
            case Roles.Doctor when appointmentToUpdate.DoctorId != request.UserId:
                return result
                    .WithError(new Error {Message = "Trying to update appointment of another doctor"})
                    .WithStatusCode(StatusCodes.Status403Forbidden);
            case Roles.Patient when appointmentToUpdate.PatientId != request.UserId:
                return result
                    .WithError(new Error {Message = "Trying to update appointment of another patient"})
                    .WithStatusCode(StatusCodes.Status403Forbidden);
        }

        if (!string.IsNullOrEmpty(request.Status) &&
            request.RoleName == Roles.Doctor &&
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
        appointmentToUpdate.Interview = request.Interview ?? appointmentToUpdate.Interview;
        appointmentToUpdate.Diagnosis = request.Diagnosis ?? appointmentToUpdate.Diagnosis;
        appointmentToUpdate.Recommendations = request.Recommendations ?? appointmentToUpdate.Recommendations;

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

            var previousStatus = appointmentToUpdate.Status.Name;
            if (_webHostEnvironment.EnvironmentName != "Development")
            {
                await SendAppointmentStatusUpdateEmailAsync(
                    doctor: appointmentToUpdate.Doctor,
                    patient: appointmentToUpdate.Patient,
                    date: appointmentToUpdate.Date,
                    previousStatus: previousStatus,
                    currentStatus: request.Status,
                    websiteAddress: _urlSettings.FrontendDomain
                );
            }

            appointmentToUpdate.Status = appointmentStatus;
        }

        var appointmentEntity = await _appointmentRepository.UpdateAsync(appointmentToUpdate);
        var appointmentResponse = _mapper.Map<AppointmentResponse>(appointmentEntity);
        return result.WithValue(appointmentResponse);
    }

    private async Task SendAppointmentStatusUpdateEmailAsync(
        Doctor doctor,
        Patient patient,
        DateTime date,
        string previousStatus,
        string currentStatus,
        string websiteAddress)
    {
        var doctorName = $"{doctor.AppUser.FirstName} {doctor.AppUser.LastName}";
        var dateString = $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        var email = new SingleEmailDto
        {
            RecipientEmail = patient.AppUser.Email,
            RecipientName = patient.AppUser.FirstName,
            TemplateId = _sendGridTemplateSettings.AppointmentStatusChange,
            TemplateData = new AppointmentStatusUpdateTemplateData
            {
                DoctorName = doctorName,
                Date = dateString,
                PreviousStatus = previousStatus,
                CurrentStatus = currentStatus,
                WebsiteAddress = websiteAddress
            }
        };
        await _sendGridService.SendSingleEmailAsync(email);
    }
}