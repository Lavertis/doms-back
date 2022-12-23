using System.Security.Claims;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.Validation;

public class UpdateTimetableValidator : AbstractValidator<UpdateTimetableBatchRequestList>
{
    public UpdateTimetableValidator(ITimetableRepository timetableRepository, IHttpContextAccessor httpContextAccessor)
    {
        CascadeMode = CascadeMode.Stop;

        var httpContext = httpContextAccessor.HttpContext!;
        var authenticatedUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Timetable batch request list cannot be empty");

        RuleForEach(x => x)
            .Must(x => x.StartDateTime < x.EndDateTime)
            .WithMessage("StartDateTime must be before EndDateTime");

        RuleForEach(x => x)
            .Must(x => x.StartDateTime >= DateTime.UtcNow)
            .WithMessage("StartDateTime must be in the future");

        RuleForEach(x => x)
            .Must(x => x.EndDateTime >= DateTime.UtcNow)
            .WithMessage("EndDateTime must be in the future");

        RuleFor(x => x)
            .Must(x =>
            {
                var timetableRequests = x.ToList();
                for (var i = 0; i < timetableRequests.Count; i++)
                {
                    for (var j = i + 1; j < timetableRequests.Count; j++)
                    {
                        if (timetableRequests[i].StartDateTime < timetableRequests[j].EndDateTime &&
                            timetableRequests[i].EndDateTime > timetableRequests[j].StartDateTime)
                        {
                            return false;
                        }
                    }
                }

                return true;
            })
            .WithMessage("Timetable requests must not overlap");

        RuleForEach(x => x)
            .MustAsync(async (_, y, cancellationToken) =>
                await timetableRepository.GetAll()
                    .Where(timetable => timetable.DoctorId.ToString() == authenticatedUserId)
                    .Where(timetable => timetable.Id != y.Id)
                    .AllAsync(timetable =>
                            timetable.StartDateTime >= y.EndDateTime || timetable.EndDateTime <= y.StartDateTime,
                        cancellationToken: cancellationToken
                    )
            )
            .WithMessage("One or more timetables overlap with existing timetables");
    }
}