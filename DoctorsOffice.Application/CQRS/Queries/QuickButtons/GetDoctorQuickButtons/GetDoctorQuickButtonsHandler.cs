using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.QuickButtons.GetDoctorQuickButtons;

public class GetDoctorQuickButtonsHandler
    : IRequestHandler<GetDoctorQuickButtonsQuery, HttpResult<DoctorQuickButtonsResponse>>
{
    private readonly IMapper _mapper;
    private readonly IQuickButtonRepository _quickButtonRepository;

    public GetDoctorQuickButtonsHandler(IQuickButtonRepository quickButtonRepository, IMapper mapper)
    {
        _quickButtonRepository = quickButtonRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<DoctorQuickButtonsResponse>> Handle(GetDoctorQuickButtonsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<DoctorQuickButtonsResponse>();
        var doctorQuickButtons = _quickButtonRepository.GetAll()
            .Where(x => x.DoctorId == request.DoctorId);

        var doctorQuickButtonsResponse = new DoctorQuickButtonsResponse
        {
            InterviewQuickButtons = await doctorQuickButtons
                .Where(x => x.Type == QuickButtonTypes.Interview)
                .OrderBy(x => x.Value)
                .Select(x => _mapper.Map<QuickButtonResponse>(x))
                .ToListAsync(cancellationToken: cancellationToken),
            DiagnosisQuickButtons = await doctorQuickButtons
                .Where(x => x.Type == QuickButtonTypes.Diagnosis)
                .OrderBy(x => x.Value)
                .Select(x => _mapper.Map<QuickButtonResponse>(x))
                .ToListAsync(cancellationToken: cancellationToken),
            RecommendationsQuickButtons = await doctorQuickButtons
                .Where(x => x.Type == QuickButtonTypes.Recommendations)
                .OrderBy(x => x.Value)
                .Select(x => _mapper.Map<QuickButtonResponse>(x))
                .ToListAsync(cancellationToken: cancellationToken)
        };
        return result.WithValue(doctorQuickButtonsResponse);
    }
}