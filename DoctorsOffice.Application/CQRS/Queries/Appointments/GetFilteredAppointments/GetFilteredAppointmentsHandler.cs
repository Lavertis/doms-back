using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetFilteredAppointments;

public class GetFilteredAppointmentsHandler
    : IRequestHandler<GetFilteredAppointmentsQuery, HttpResult<IEnumerable<AppointmentSearchResponse>>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetFilteredAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<AppointmentSearchResponse>>> Handle(
        GetFilteredAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<AppointmentSearchResponse>>();

        var appointmentsQueryable = _appointmentRepository.GetAll()
            .Include(a => a.Type)
            .Include(a => a.Status)
            .Include(a => a.Patient).ThenInclude(p => p.AppUser)
            .AsQueryable();

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
            .OrderBy(appointment => appointment.Date)
            .Select(appointment => _mapper.Map<AppointmentSearchResponse>(appointment))
            .ToListAsync(cancellationToken: cancellationToken);

        return result.WithValue(appointmentResponses);
    }
}