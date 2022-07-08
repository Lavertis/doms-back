using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdHandler : IRequestHandler<GetAppointmentsByPatientIdQuery, IList<AppointmentResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetAppointmentsByPatientIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<AppointmentResponse>> Handle(GetAppointmentsByPatientIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = _dbContext.Appointments
            .Where(a => a.Patient.Id == request.PatientId)
            .OrderBy(a => a.Date);

        return await appointments.Select(appointment => new AppointmentResponse(appointment)).ToListAsync(cancellationToken: cancellationToken);
    }
}