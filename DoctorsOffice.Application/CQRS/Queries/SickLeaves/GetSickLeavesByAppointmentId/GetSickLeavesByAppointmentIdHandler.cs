using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetSickLeavesByAppointmentId;

public class GetSickLeavesByAppointmentIdHandler : IRequestHandler<GetSickLeavesByAppointmentIdQuery,
    HttpResult<PagedResponse<SickLeaveResponse>>>
{
    private readonly IMapper _mapper;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public GetSickLeavesByAppointmentIdHandler(ISickLeaveRepository sickLeaveRepository, IMapper mapper)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<SickLeaveResponse>>> Handle(GetSickLeavesByAppointmentIdQuery request,
        CancellationToken cancellationToken)
    {
        var sickLeavesQueryable = _sickLeaveRepository.GetAll()
            .Where(sickLeave => sickLeave.AppointmentId == request.AppointmentId);

        if (request.DoctorId != null)
            sickLeavesQueryable = sickLeavesQueryable.Where(p => p.DoctorId == request.DoctorId);
        if (request.PatientId != null)
            sickLeavesQueryable = sickLeavesQueryable.Where(p => p.PatientId == request.PatientId);

        var sickLeaveResponsesQueryable = sickLeavesQueryable.Select(s => _mapper.Map<SickLeaveResponse>(s));

        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(sickLeaveResponsesQueryable, request.PaginationFilter)
        );
    }
}