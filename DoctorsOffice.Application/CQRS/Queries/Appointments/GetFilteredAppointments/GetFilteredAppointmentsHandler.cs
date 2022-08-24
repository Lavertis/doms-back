using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetFilteredAppointments;

public class GetFilteredAppointmentsHandler
    : IRequestHandler<GetFilteredAppointmentsQuery, HttpResult<PagedResponse<AppointmentSearchResponse>>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetFilteredAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<PagedResponse<AppointmentSearchResponse>>> Handle(
        GetFilteredAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<PagedResponse<AppointmentSearchResponse>>();
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
            appointmentsQueryable = appointmentsQueryable.Where(a => a.PatientId == request.PatientId);
        if (request.DoctorId is not null)
            appointmentsQueryable = appointmentsQueryable.Where(a => a.DoctorId == request.DoctorId);

        var responsesQueryable = appointmentsQueryable
            .OrderBy(a => a.Date)
            .Select(a => _mapper.Map<AppointmentSearchResponse>(a));

        var pagedResult = PaginationUtils.CreatePagedResponse(
            recordsQueryable: responsesQueryable,
            paginationFilter: request.PaginationFilter,
            totalRecords: await appointmentsQueryable.CountAsync(cancellationToken: cancellationToken)
        );
        if (pagedResult.IsFailed)
            return result
                .WithError(pagedResult.Error)
                .WithStatusCode(StatusCodes.Status400BadRequest);

        return result.WithValue(pagedResult.Value!);
    }
}