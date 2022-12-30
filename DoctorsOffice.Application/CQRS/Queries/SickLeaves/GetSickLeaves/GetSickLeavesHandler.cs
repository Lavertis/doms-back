using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetSickLeaves;

public class GetSickLeavesHandler : IRequestHandler<GetSickLeavesQuery, HttpResult<PagedResponse<SickLeaveResponse>>>
{
    private readonly IMapper _mapper;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public GetSickLeavesHandler(ISickLeaveRepository sickLeaveRepository, IMapper mapper)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<SickLeaveResponse>>> Handle(
        GetSickLeavesQuery request, CancellationToken cancellationToken)
    {
        var sickLeavesResponsesQueryable = _sickLeaveRepository.GetAll()
            .Select(s => _mapper.Map<SickLeaveResponse>(s));

        return Task.FromResult(PaginationUtils.CreatePagedHttpResult(
            sickLeavesResponsesQueryable,
            request.PaginationFilter
        ));
    }
}