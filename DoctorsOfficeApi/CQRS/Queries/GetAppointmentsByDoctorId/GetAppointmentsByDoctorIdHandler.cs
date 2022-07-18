using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AppointmentRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdHandler : IRequestHandler<GetAppointmentsByDoctorIdQuery, IList<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByDoctorIdHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IList<AppointmentResponse>> Handle(GetAppointmentsByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = _appointmentRepository.GetAll(
                a => a.Doctor,
                a => a.Patient,
                a => a.Type,
                a => a.Status)
            .Where(a => a.Doctor.Id == request.DoctorId)
            .OrderBy(a => a.Date);

        return await appointments.Select(appointment => new AppointmentResponse(appointment)).ToListAsync(cancellationToken: cancellationToken);
    }
}