using DoctorsOffice.Application.CQRS.Commands.Appointments.CreateAppointment;
using DoctorsOffice.Application.CQRS.Commands.Appointments.UpdateAppointment;
using DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;
using DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;
using DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;
using DoctorsOffice.Application.CQRS.Commands.Patients.UpdatePatientById;
using DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;
using DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;
using DoctorsOffice.Domain.DTO.Requests;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.Validation;

public static class FluentValidationModule
{
    public static void AddFluentValidationModule(this IServiceCollection services)
    {
        services.AddFluentValidation();
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