using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Statistics.GetDoctorStatisticsByDate;

public class
    GetDoctorStatisticsByDateHandler : IRequestHandler<GetDoctorStatisitcsByDateQuery,
        HttpResult<DoctorStatisticsResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetDoctorStatisticsByDateHandler(IAppointmentRepository appointmentRepository,
        IPrescriptionRepository prescriptionRepository)
    {
        _appointmentRepository = appointmentRepository;
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<HttpResult<DoctorStatisticsResponse>> Handle(GetDoctorStatisitcsByDateQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<DoctorStatisticsResponse>();

        var dateStart = request.DateStart ?? DateTime.MinValue.ToUniversalTime();
        var dateEnd = request.DateEnd ?? DateTime.UtcNow;

        // TODO these value repeats in get admin statistics. Refactor
        var appointmentsCount = await _appointmentRepository.GetAll()
            .Where(a => a.DoctorId == request.DoctorId)
            .CountAsync(a => a.Date >= dateStart && a.Date <= dateEnd, cancellationToken);

        // TODO these value repeats in get admin statistics. Refactor
        var writtenPrescriptionsCount = await _prescriptionRepository.GetAll()
            .Where(a => a.DoctorId == request.DoctorId)
            .CountAsync(p => p.CreatedAt >= dateStart && p.CreatedAt <= dateEnd, cancellationToken);

        // TODO these value repeats in get admin statistics. Refactor
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
        var patientsCountByAppointments = await _appointmentRepository.GetAll()
            .Where(a => a.DoctorId == request.DoctorId)
            .Where(a => a.Date >= dateStart && a.Date <= dateEnd)
            .GroupBy(a => a.PatientId)
            .Select(g => g.Key)
            .CountAsync(cancellationToken);

        var response = new DoctorStatisticsResponse
        {
            AppointmentsCount = appointmentsCount,
            PrescriptionsCount = writtenPrescriptionsCount,
            AppointmentTypesCount = appointmentCountPerType,
            DoctorsPatientCount = 1,
        };
        return result.WithValue(response);
    }
}