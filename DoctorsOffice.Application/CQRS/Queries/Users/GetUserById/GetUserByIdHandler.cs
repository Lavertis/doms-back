using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, HttpResult<UserResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IMapper _mapper;

    public GetUserByIdHandler(IMapper mapper, AppUserManager appUserManager)
    {
        _mapper = mapper;
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<UserResponse>();
        var findByIdResult = await _appUserManager.FindByIdAsync(request.UserId);
        if (findByIdResult.IsError || findByIdResult.Value is null)
        {
            return result
                .WithError(findByIdResult.Error)
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var user = findByIdResult.Value;
        return result.WithValue(_mapper.Map<UserResponse>(user));
    }
}