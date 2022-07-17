using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Services.AppointmentService;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _dbContext;

    public AppointmentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Appointment> GetAppointmentByIdAsync(Guid id)
    {
        var appointment = await _dbContext.Appointments.FindAsync(id);
        if (appointment is null)
        {
            throw new NotFoundException("Appointment not found");
        }

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

    public async Task<AppointmentStatus> GetAppointmentStatusByNameAsync(string status)
    {
        var appointmentStatus = await _dbContext.AppointmentStatuses.SingleOrDefaultAsync(s => s.Name == status);
        if (appointmentStatus is null)
        {
            throw new NotFoundException("Appointment status not found");
        }

        return appointmentStatus;
    }

    public async Task<AppointmentType> GetAppointmentTypeByNameAsync(string type)
    {
        var appointmentType = await _dbContext.AppointmentTypes.SingleOrDefaultAsync(t => t.Name == type);
        if (appointmentType is null)
        {
            throw new NotFoundException("Appointment type not found");
        }

        return appointmentType;
    }
}