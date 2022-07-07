using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Models.Requests;

namespace DoctorsOfficeApi.Services.AppointmentService;

public interface IAppointmentService
{
    public Task<IList<Appointment>> GetAllAppointmentsAsync();

    public Task<IList<Appointment>> GetAppointmentsByPatientIdAsync(string patientId);

    public Task<IList<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId);

    public Task<Appointment> GetAppointmentByIdAsync(long id);

    public Task<IList<Appointment>> GetFilteredAppointmentsAsync(
        DateTime? dateStart,
        DateTime? dateEnd,
        string? type,
        string? status,
        string? patientId,
        string? doctorId);

    public Task<Appointment> CreateAppointmentAsync(CreateAppointmentRequest request);

    public Task<Appointment> UpdateAppointmentByIdAsync(long id, UpdateAppointmentRequest request);

    public Task<Appointment> CancelAppointmentByIdAsync(long id);

    public Task<bool> AppointmentTypeExistsAsync(string type);

    public Task<bool> AppointmentStatusExistsAsync(string status);
}