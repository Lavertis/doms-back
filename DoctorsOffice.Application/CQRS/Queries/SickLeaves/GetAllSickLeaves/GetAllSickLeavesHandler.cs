using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetAllSickLeaves;

public class GetAllSickLeavesHandler : IRequestHandler<GetAllSickLeavesQuery, HttpResult<PagedResponse<SickLeaveResponse>>>
{
    private readonly ISickLeaveRepository _sickLeaveRepository;
    private readonly IMapper _mapper;

    public GetAllSickLeavesHandler(ISickLeaveRepository sickLeaveRepository, IMapper mapper)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<SickLeaveResponse>>> Handle(
        GetAllSickLeavesQuery request, CancellationToken cancellationToken)
    {
        var sickLeavesResponsesQuerable = _sickLeaveRepository.GetAll()
            .Select(s => _mapper.Map<SickLeaveResponse>(s));

        return Task.FromResult(PaginationUtils.CreatePagedHttpResult(
            sickLeavesResponsesQuerable,
            request.PaginationFilter
        ));
    }
}