using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetAllSickLeavesByPatientId;

public class GetSickLeavesByPatientIdHandler : IRequestHandler<GetSickLeavesByPatientIdQuery, HttpResult<PagedResponse<SickLeaveResponse>>>
{
    private readonly ISickLeaveRepository _sickLeaveRepository;
    private readonly IMapper _mapper;

    public GetSickLeavesByPatientIdHandler(ISickLeaveRepository sickLeaveRepository, IMapper mapper)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<SickLeaveResponse>>> Handle(GetSickLeavesByPatientIdQuery request, CancellationToken cancellationToken)
    {
         var sickLeaves = _sickLeaveRepository.GetAll()
            .Where(s => s.PatientId == request.PatientId)
            .OrderBy(s => s.DateStart);

        var sickLeavesResponsesQueryable = sickLeaves
            .Select(s => _mapper.Map<SickLeaveResponse>(s));
        
        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(sickLeavesResponsesQueryable, request.PaginationFilter)
        );
    }
}