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

    public async Task<IList<AppointmentResponse>> Handle(GetFilteredAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var appointmentsQueryable = _appointmentRepository.GetAll(
            a => a.Doctor,
            a => a.Patient,
            a => a.Type,
            a => a.Status
        );
        if (request.DateStart is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Date >= request.DateStart);
        if (request.DateEnd is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Date <= request.DateEnd);
        if (request.Type is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Type.Name == request.Type);
        if (request.Status is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Status.Name == request.Status);
        if (request.PatientId is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Patient.Id == request.PatientId);
        if (request.DoctorId is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.Doctor.Id == request.DoctorId);

        var appointmentResponses = await appointmentsQueryable
            .OrderBy(a => a.Date)
            .Select(a => new AppointmentResponse(a))
            .ToListAsync(cancellationToken: cancellationToken);

        return appointmentResponses;
    }
}