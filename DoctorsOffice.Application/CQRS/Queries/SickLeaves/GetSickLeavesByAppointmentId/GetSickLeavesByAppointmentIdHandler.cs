using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        var sickLeaveResponsesQueryable = _sickLeaveRepository.GetAll()
            .Where(p => p.DoctorId == request.DoctorId && p.AppointmentId == request.AppointmentId)
            .Select(p => _mapper.Map<SickLeaveResponse>(p));

        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(sickLeaveResponsesQueryable, request.PaginationFilter)
        );
    }
}