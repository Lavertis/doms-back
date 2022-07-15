using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetFilteredAppointments;

public class GetFilteredAppointmentsHandler : IRequestHandler<GetFilteredAppointmentsQuery, IList<AppointmentResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetFilteredAppointmentsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<AppointmentResponse>> Handle(GetFilteredAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointmentsQueryable = _dbContext.Appointments.AsQueryable();
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