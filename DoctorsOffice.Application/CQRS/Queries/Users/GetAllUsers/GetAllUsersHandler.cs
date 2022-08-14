using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, HttpResult<IEnumerable<UserResponse>>>
{
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;

    public GetAllUsersHandler(UserManager<AppUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<UserResponse>>> Handle(GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<UserResponse>>();
        var users = await _userManager.Users.ToListAsync(cancellationToken: cancellationToken);
        var userResponses = users.Select(user => _mapper.Map<UserResponse>(user));
        return result.WithValue(userResponses);
    }
}