using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdHandler : IRequestHandler<GetAppointmentsByDoctorIdQuery, IList<AppointmentResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetAppointmentsByDoctorIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<AppointmentResponse>> Handle(GetAppointmentsByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = _dbContext.Appointments
            .Where(a => a.Doctor.Id == request.DoctorId)
            .OrderBy(a => a.Date);

        return await appointments.Select(appointment => new AppointmentResponse(appointment)).ToListAsync(cancellationToken: cancellationToken);
    }
}