using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IList<UserResponse>>
{
    private readonly UserManager<AppUser> _userManager;

    public GetAllUsersHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IList<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken: cancellationToken);
        var userResponses = users.Select(user => new UserResponse(user));
        return userResponses.ToList();
    }
}