using DoctorsOffice.Domain.DTO.Requests;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.Validation;

public static class FluentValidationModule
{
    public static void AddFluentValidators(this IServiceCollection services)
    {
        services.AddFluentValidation();
        services.AddScoped<IValidator<AuthenticateRequest>, AuthenticateRequestValidator>();
        services.AddScoped<IValidator<CreateAppointmentRequest>, CreateAppointmentRequestValidator>();
        services.AddScoped<IValidator<UpdateAppointmentRequest>, UpdateAppointmentRequestValidator>();
        services.AddScoped<IValidator<CreatePatientRequest>, CreatePatientRequestValidator>();
        services.AddScoped<IValidator<UpdateAuthenticatedPatientRequest>, UpdateAuthenticatedPatientRequestValidator>();
        services.AddScoped<IValidator<CreateDoctorRequest>, CreateDoctorRequestValidator>();
        services.AddScoped<IValidator<UpdateAuthenticatedDoctorRequest>, UpdateAuthenticatedDoctorRequestValidator>();
        services.AddScoped<IValidator<UpdateDoctorRequest>, UpdateDoctorRequestValidator>();
        services.AddScoped<IValidator<CreatePrescriptionRequest>, CreatePrescriptionRequestValidator>();
        services.AddScoped<IValidator<UpdatePrescriptionRequest>, UpdatePrescriptionRequestValidator>();
    }
}