using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AppointmentRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetFilteredAppointments;

public class GetFilteredAppointmentsHandler : IRequestHandler<GetFilteredAppointmentsQuery, IList<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetFilteredAppointmentsHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IList<AppointmentResponse>> Handle(GetFilteredAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointmentsQueryable = _appointmentRepository.GetAll(
            a => a.Doctor,
            a => a.Patient,
            a => a.Type,
            a => a.Status
        );
        if (request.dateStart is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Date >= request.dateStart);
        if (request.dateEnd is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Date <= request.dateEnd);
        if (request.type is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Type.Name == request.type);
        if (request.status is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Status.Name == request.status);
        if (request.patientId is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Patient.Id == request.patientId);
        if (request.doctorId is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Doctor.Id == request.doctorId);

        var appointmentResponses = await appointmentsQueryable
            .OrderBy(a => a.Date)
            .Select(a => new AppointmentResponse(a))
            .ToListAsync(cancellationToken: cancellationToken);

        return appointmentResponses;
    }
}