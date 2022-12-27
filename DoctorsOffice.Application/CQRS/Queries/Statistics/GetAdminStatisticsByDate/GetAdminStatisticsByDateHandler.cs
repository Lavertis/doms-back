using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Statistics.GetAdminStatisticsByDate;

public class
    GetAdminStatisticsByDateHandler : IRequestHandler<GetAdminStatisitcsByDateQuery,
        HttpResult<AdminStatisticsResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly AppUserManager _appUserManager;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetAdminStatisticsByDateHandler(AppUserManager appUserManager, IAppointmentRepository appointmentRepository,
        IPrescriptionRepository prescriptionRepository, IDoctorRepository doctorRepository,
        IPatientRepository patientRepository)
    {
        _appUserManager = appUserManager;
        _appointmentRepository = appointmentRepository;
        _prescriptionRepository = prescriptionRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    public async Task<HttpResult<AdminStatisticsResponse>> Handle(GetAdminStatisitcsByDateQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<AdminStatisticsResponse>();

        var dateStart = request.DateStart ?? DateTime.MinValue.ToUniversalTime();
        var dateEnd = request.DateEnd ?? DateTime.Now.ToUniversalTime();

        // TODO these value repeats in get doctor statistics. Refactor
        var appointmentsCount = await _appointmentRepository.GetAll()
            .Where(a => a.DoctorId == request.DoctorId)
            .CountAsync(a => a.Date >= dateStart && a.Date <= dateEnd, cancellationToken);

        // TODO these value repeats in get doctor statistics. Refactor
        var writtenPrescriptionsCount = await _prescriptionRepository.GetAll()
            .Where(a => a.DoctorId == request.DoctorId)
            .CountAsync(p => p.CreatedAt >= dateStart && p.CreatedAt <= dateEnd, cancellationToken);

        // TODO these value repeats in get doctor statistics. Refactor
        var appointmentCountPerType = await _appointmentRepository.GetAll()
            .Include(a => a.Type)
            .Where(a => a.DoctorId == request.DoctorId)
            .Where(a => a.Date >= dateStart && a.Date <= dateEnd)
            .GroupBy(
                a => a.Type.Name,
                (type, appointments) => new
                {
                    Key = type,
                    Count = appointments.Count()
                })
            .ToDictionaryAsync(a => a.Key, a => a.Count, cancellationToken);

        // TODO this should also take into account prescriptions not assigned to appointments
        var doctorsPatientCountByAppointments = await _appointmentRepository.GetAll()
            .Where(a => a.DoctorId == request.DoctorId)
            .Where(a => a.Date >= dateStart && a.Date <= dateEnd)
            .GroupBy(a => a.PatientId)
            .Select(g => g.Key)
            .CountAsync(cancellationToken);

        var doctorCount = _doctorRepository.GetAll().Count();
        var patientCount = _patientRepository.GetAll().Count();
        var userCount = _appUserManager.Users.Count();

        var response = new AdminStatisticsResponse
        {
            AppointmentsCount = appointmentsCount,
            PrescriptionsCount = writtenPrescriptionsCount,
            AppointmentTypesCount = appointmentCountPerType,
            DoctorsPatientCount = doctorsPatientCountByAppointments,
            DoctorCount = doctorCount,
            PatientCount = patientCount,
            UserCount = userCount
        };
        return result.WithValue(response);
    }
}