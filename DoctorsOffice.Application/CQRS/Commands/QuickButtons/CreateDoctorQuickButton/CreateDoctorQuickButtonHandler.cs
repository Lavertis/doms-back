using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.QuickButtons.CreateDoctorQuickButton;

public class CreateDoctorQuickButtonHandler
    : IRequestHandler<CreateDoctorQuickButtonCommand, HttpResult<QuickButtonResponse>>
{
    private readonly IMapper _mapper;
    private readonly IQuickButtonRepository _quickButtonRepository;

    public CreateDoctorQuickButtonHandler(IQuickButtonRepository quickButtonRepository, IMapper mapper)
    {
        _quickButtonRepository = quickButtonRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<QuickButtonResponse>> Handle(CreateDoctorQuickButtonCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<QuickButtonResponse>();
        var data = request.Data;
        var quickButton = await _quickButtonRepository.GetAll()
            .SingleOrDefaultAsync(quickButton =>
                    quickButton.DoctorId == request.DoctorId &&
                    quickButton.Type == data.Type &&
                    quickButton.Value == data.Value,
                cancellationToken: cancellationToken
            );
        if (quickButton != null)
        {
            return result
                .WithError(new Error {Message = "QuickButton with that value and type already exists"})
                .WithStatusCode(StatusCodes.Status409Conflict);
        }

        quickButton = new QuickButton
        {
            DoctorId = request.DoctorId,
            Value = data.Value,
            Type = data.Type
        };
        var quickButtonEntity = await _quickButtonRepository.CreateAsync(quickButton);
        return result
            .WithValue(_mapper.Map<QuickButtonResponse>(quickButtonEntity))
            .WithStatusCode(StatusCodes.Status201Created);
    }
}