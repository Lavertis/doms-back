using DoctorsOfficeApi.Models.Requests;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace DoctorsOfficeApi.Modules;

public static class Validators
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddFluentValidation();
        services.AddTransient<IValidator<AuthenticateRequest>, AuthenticateRequestValidator>();
        services.AddTransient<IValidator<CreateAppointmentRequest>, CreateAppointmentRequestValidator>();
        services.AddTransient<IValidator<UpdateAppointmentRequest>, UpdateAppointmentRequestValidator>();
        services.AddTransient<IValidator<CreatePatientRequest>, CreatePatientRequestValidator>();
        services
            .AddTransient<IValidator<UpdateAuthenticatedPatientRequest>, UpdateAuthenticatedPatientRequestValidator>();
        services.AddTransient<IValidator<CreateDoctorRequest>, CreateDoctorRequestValidator>();
        services
            .AddTransient<IValidator<UpdateAuthenticatedDoctorRequest>, UpdateAuthenticatedDoctorRequestValidator>();
        services.AddTransient<IValidator<UpdateDoctorRequest>, UpdateDoctorRequestValidator>();
        services.AddTransient<IValidator<CreatePrescriptionRequest>, CreatePrescriptionRequestValidator>();
        services.AddTransient<IValidator<UpdatePrescriptionRequest>, UpdatePrescriptionRequestValidator>();
    }
}