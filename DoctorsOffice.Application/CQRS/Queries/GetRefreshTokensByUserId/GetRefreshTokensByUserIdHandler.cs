﻿using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.Entities;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetRefreshTokensByUserId;

public class GetRefreshTokensByUserIdHandler : IRequestHandler<GetRefreshTokensByUserIdQuery, IList<RefreshToken>>
{
    private readonly IUserService _userService;

    public GetRefreshTokensByUserIdHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IList<RefreshToken>> Handle(GetRefreshTokensByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(request.UserId);
        var refreshTokens = user.RefreshTokens.ToList();
        return refreshTokens;
    }
}