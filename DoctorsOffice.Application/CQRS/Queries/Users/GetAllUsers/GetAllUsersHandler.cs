using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, HttpResult<IEnumerable<UserResponse>>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IMapper _mapper;

    public GetAllUsersHandler(AppUserManager appUserManager, IMapper mapper)
    {
        _appUserManager = appUserManager;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<UserResponse>>> Handle(GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<UserResponse>>();
        var users = await _appUserManager.Users.ToListAsync(cancellationToken: cancellationToken);
        var userResponses = users.Select(user => _mapper.Map<UserResponse>(user));
        return result.WithValue(userResponses);
    }
}