using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class CreateQuickButtonValidator : AbstractValidator<CreateQuickButtonRequest>
{
    private readonly IQuickButtonRepository _quickButtonRepository;

    public CreateQuickButtonValidator(IQuickButtonRepository quickButtonRepository)
    {
        _quickButtonRepository = quickButtonRepository;
        RuleFor(e => e.Type)
            .NotEmpty()
            .Must(type =>
                type is QuickButtonTypes.Interview or QuickButtonTypes.Diagnosis or QuickButtonTypes.Recommendations)
            .WithMessage("Invalid type");

        RuleFor(e => e.Value)
            .NotEmpty();
    }
}