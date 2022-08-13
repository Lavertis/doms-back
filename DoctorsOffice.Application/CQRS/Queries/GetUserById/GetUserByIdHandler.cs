using DoctorsOffice.Application.Services.User;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetUserById;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUserService _userService;

    public GetUserByIdHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(request.UserId);
        return new UserResponse(user);
    }
}