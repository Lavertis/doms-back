using System.Security.Claims;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.Validation;

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

        RuleFor(e => e.Type)
            .MustAsync((type, _) => appointmentTypeRepository.ExistsByNameAsync(type!))
            .WithMessage("Type does not exist")
            .Unless(e => string.IsNullOrEmpty(e.Type));

        When(req => !string.IsNullOrEmpty(req.Status), () =>
        {
            RuleFor(e => e.Status)
                .MustAsync((status, _) => appointmentStatusRepository.ExistsByNameAsync(status!))
                .WithMessage("Status does not exist");

            RuleFor(e => e.Status)
                .Must(status => status != AppointmentStatuses.Pending)
                .WithMessage("Status does not exist");

            RuleFor(e => e.Status)
                .Must(status => status == AppointmentStatuses.Cancelled)
                .WithMessage("Only allowed status is Cancelled")
                .When(e => userRole == RoleTypes.Patient);
        });
    }
}