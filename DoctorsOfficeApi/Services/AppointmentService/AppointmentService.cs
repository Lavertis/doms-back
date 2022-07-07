using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using Microsoft.EntityFrameworkCore;
using AppointmentStatusEntity = DoctorsOfficeApi.Entities.AppointmentStatus;

namespace DoctorsOfficeApi.Services.AppointmentService;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _dbContext;

    public AppointmentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<Appointment>> GetAllAppointmentsAsync()
    {
        return await _dbContext.Appointments.OrderBy(a => a.Date).ToListAsync();
    }

    public async Task<IList<Appointment>> GetAppointmentsByPatientIdAsync(string patientId)
    {
        var appointments = _dbContext.Appointments
            .Where(a => a.Patient.Id == patientId)
            .OrderBy(a => a.Date);

        return await appointments.ToListAsync();
    }

    public async Task<IList<Appointment>> GetAppointmentsByDoctorIdAsync(string patientId)
    {
        var appointments = _dbContext.Appointments
            .Where(a => a.Doctor.Id == patientId)
            .OrderBy(a => a.Date);

        return await appointments.ToListAsync();
    }

    public async Task<Appointment> GetAppointmentByIdAsync(long id)
    {
        var appointment = await _dbContext.Appointments.FindAsync(id);
        if (appointment == null)
        {
            throw new NotFoundException("Appointment not found");
        }

        return appointment;
    }

    public async Task<IList<Appointment>> GetFilteredAppointmentsAsync(
        DateTime? dateStart,
        DateTime? dateEnd,
        string? type,
        string? status,
        string? patientId,
        string? doctorId)
    {
        var appointmentsQueryable = _dbContext.Appointments.AsQueryable();
        if (dateStart != null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Date >= dateStart);
        if (dateEnd != null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Date <= dateEnd);
        if (type != null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Type.Name == type);
        if (status != null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Status.Name == status);
        if (patientId != null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Patient.Id == patientId);
        if (doctorId != null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Doctor.Id == doctorId);

        return await appointmentsQueryable.OrderBy(a => a.Date).ToListAsync();
    }

    public async Task<Appointment> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        Appointment appointment;
        try
        {
            appointment = new Appointment
            {
                Date = request.Date,
                Description = request.Description,
                Patient = (await _dbContext.Patients.FindAsync(request.PatientId))!, // TODO replace with patient service method
                Doctor = (await _dbContext.Doctors.FindAsync(request.DoctorId))!, // TODO replace with patient service method
                Status = await GetAppointmentStatusByNameAsync(request.Status!),
                Type = await GetAppointmentTypeByNameAsync(request.Type),
            };
        }
        catch (NotFoundException e)
        {
            throw new BadRequestException(e.Message);
        }

        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> UpdateAppointmentByIdAsync(long id, UpdateAppointmentRequest request)
    {
        var appointment = await GetAppointmentByIdAsync(id);
        appointment.Date = request.Date ?? appointment.Date;
        appointment.Description = request.Description ?? appointment.Description;
        try
        {
            if (request.Type != null)
                appointment.Type = await GetAppointmentTypeByNameAsync(request.Type);
            if (request.Status != null)
                appointment.Status = await GetAppointmentStatusByNameAsync(request.Status);
        }
        catch (NotFoundException e)
        {
            throw new BadRequestException(e.Message);
        }

        _dbContext.Appointments.Update(appointment);
        await _dbContext.SaveChangesAsync();

        return appointment;
    }

    public async Task<Appointment> CancelAppointmentByIdAsync(long id)
    {
        var appointment = await _dbContext.Appointments.FindAsync(id);
        if (appointment == null)
        {
            throw new NotFoundException("Appointment not found");
        }

        var canceledStatus = await GetAppointmentStatusByNameAsync(AppointmentStatuses.Cancelled);
        appointment.Status = canceledStatus;

        _dbContext.Appointments.Update(appointment);
        await _dbContext.SaveChangesAsync();
        return appointment;
    }

    public async Task<bool> AppointmentTypeExistsAsync(string type)
    {
        return await _dbContext.AppointmentTypes.AnyAsync(a => a.Name == type);
    }

    public async Task<bool> AppointmentStatusExistsAsync(string status)
    {
        return await _dbContext.AppointmentStatuses.AnyAsync(a => a.Name == status);
    }

    private async Task<AppointmentStatusEntity> GetAppointmentStatusByNameAsync(string status)
    {
        var appointmentStatus = await _dbContext.AppointmentStatuses.SingleOrDefaultAsync(s => s.Name == status);
        if (appointmentStatus == null)
        {
            throw new NotFoundException("Appointment status not found");
        }

        return appointmentStatus;
    }

    private async Task<AppointmentType> GetAppointmentTypeByNameAsync(string type)
    {
        var appointmentType = await _dbContext.AppointmentTypes.SingleOrDefaultAsync(t => t.Name == type);
        if (appointmentType == null)
        {
            throw new NotFoundException("Appointment type not found");
        }

        return appointmentType;
    }
}