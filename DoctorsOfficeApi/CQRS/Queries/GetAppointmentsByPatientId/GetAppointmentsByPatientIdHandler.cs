using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AppointmentRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdHandler : IRequestHandler<GetAppointmentsByPatientIdQuery, IList<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByPatientIdHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IList<AppointmentResponse>> Handle(GetAppointmentsByPatientIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = _appointmentRepository.GetAll(
                a => a.Doctor,
                a => a.Patient,
                a => a.Type,
                a => a.Status)
            .Where(a => a.Patient.Id == request.PatientId)
            .OrderBy(a => a.Date);

        return await appointments.Select(appointment => new AppointmentResponse(appointment)).ToListAsync(cancellationToken: cancellationToken);
    }
}