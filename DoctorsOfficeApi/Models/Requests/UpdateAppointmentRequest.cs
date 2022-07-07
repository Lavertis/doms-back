using System.Security.Claims;
using DoctorsOfficeApi.Services.AppointmentService;
using FluentValidation;

namespace DoctorsOfficeApi.Models.Requests;

public class UpdateAppointmentRequest
{
    public DateTime? Date { get; set; }
    public string? Description { get; set; } = default!;
    public string? Type { get; set; } = default!;

    public string? Status { get; set; } = default!;
}

public class UpdateAppointmentRequestValidator : AbstractValidator<UpdateAppointmentRequest>
{
    public UpdateAppointmentRequestValidator(IAppointmentService appointmentService, IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var userRole = httpContext.User.FindFirst(ClaimTypes.Role)!.Value;

        RuleFor(e => e.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date must be in the future")
            .Unless(d => d.Date != null);

        RuleFor(e => e.Description);

        RuleFor(e => e.Type)
            .MustAsync((type, cancellationToken) => appointmentService.AppointmentTypeExistsAsync(type!))
            .WithMessage("Type does not exist")
            .Unless(e => string.IsNullOrEmpty(e.Type));

        When(req => !string.IsNullOrEmpty(req.Status), () =>
        {
            RuleFor(e => e.Status)
                .MustAsync((status, cancellationToken) => appointmentService.AppointmentStatusExistsAsync(status!))
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