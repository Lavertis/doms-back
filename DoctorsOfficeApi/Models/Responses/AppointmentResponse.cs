﻿using DoctorsOfficeApi.Entities;

namespace DoctorsOfficeApi.Models.Responses;

public class AppointmentResponse
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public string DoctorId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Type { get; set; } = default!;

    public AppointmentResponse(Appointment appointment)
    {
        Id = appointment.Id;
        Date = appointment.Date;
        Description = appointment.Description;
        PatientId = appointment.Patient.Id;
        DoctorId = appointment.Doctor.Id;
        Status = appointment.Status.Name;
        Type = appointment.Type.Name;
    }

    public AppointmentResponse()
    {
    }
}