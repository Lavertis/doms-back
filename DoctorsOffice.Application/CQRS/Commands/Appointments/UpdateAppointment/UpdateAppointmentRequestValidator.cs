using System.Security.Claims;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Appointments.UpdateAppointment;

public class UpdateAppointmentRequestValidator : AbstractValidator<UpdateAppointmentRequest>
{
    public UpdateAppointmentRequestValidator(
        IHttpContextAccessor httpContextAccessor,
        IAppointmentStatusRepository appointmentStatusRepository,
        IAppointmentTypeRepository appointmentTypeRepository)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var userRole = httpContext.User.FindFirst(ClaimTypes.Role)!.Value;

        RuleFor(e => e.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date must be in the future")
            .Unless(d => d.Date is not null);

        RuleFor(e => e.Description);

        RuleFor(e => e.TypeId)
            .MustAsync((typeId, _) => appointmentTypeRepository.ExistsByIdAsync(typeId!.Value))
            .WithMessage("Type does not exist")
            .Unless(e => e.TypeId is null);

        When(req => req.StatusId is not null, () =>
        {
            RuleFor(e => e.StatusId)
                .MustAsync((statusId, _) => appointmentStatusRepository.ExistsByIdAsync(statusId!.Value))
                .WithMessage("Status does not exist");

            RuleFor(e => e.StatusId)
                .Must(statusId => statusId != AppointmentStatuses.Pending.Id)
                .WithMessage("Status does not exist");

            RuleFor(e => e.StatusId)
                .Must(statusId => statusId == AppointmentStatuses.Cancelled.Id)
                .WithMessage("Only allowed status is Cancelled")
                .When(e => userRole == Roles.Patient);
        });
    }
}