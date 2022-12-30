using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetSickLeavesByDoctorId;

public class GetSickLeavesByDoctorIdHandler : IRequestHandler<GetSickLeavesByDoctorIdQuery,
    HttpResult<PagedResponse<SickLeaveResponse>>>
{
    private readonly IMapper _mapper;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public GetSickLeavesByDoctorIdHandler(ISickLeaveRepository sickLeaveRepository, IMapper mapper)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<SickLeaveResponse>>> Handle(GetSickLeavesByDoctorIdQuery request,
        CancellationToken cancellationToken)
    {
        var sickLeaves = _sickLeaveRepository.GetAll()
            .Where(s => s.DoctorId == request.DoctorId)
            .OrderBy(s => s.DateStart);

        var sickLeavesResponsesQueryable = sickLeaves
            .Select(s => _mapper.Map<SickLeaveResponse>(s));

        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(sickLeavesResponsesQueryable, request.PaginationFilter)
        );
    }

    private HttpResult<PagedResponse<SickLeaveResponse>> GetPatientSickLeavesPagedResult(
        Guid patientId, PaginationFilter? paginationFilter)
    {
        var sickLeaves = _sickLeaveRepository.GetAll()
            .Where(s => s.PatientId == patientId)
            .OrderBy(s => s.DateStart);

        var sickLeavesResponsesQueryable = sickLeaves
            .Select(s => _mapper.Map<SickLeaveResponse>(s));

        return PaginationUtils.CreatePagedHttpResult(sickLeavesResponsesQueryable, paginationFilter);
    }
}