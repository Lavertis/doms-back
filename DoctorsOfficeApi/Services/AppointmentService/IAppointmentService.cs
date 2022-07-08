using DoctorsOfficeApi.Entities;

namespace DoctorsOfficeApi.Services.AppointmentService;

public interface IAppointmentService
{
    public Task<Appointment> GetAppointmentByIdAsync(long id);
    public Task<bool> AppointmentTypeExistsAsync(string type);

    public Task<bool> AppointmentStatusExistsAsync(string status);
    public Task<AppointmentStatus> GetAppointmentStatusByNameAsync(string status);
    public Task<AppointmentType> GetAppointmentTypeByNameAsync(string type);
}