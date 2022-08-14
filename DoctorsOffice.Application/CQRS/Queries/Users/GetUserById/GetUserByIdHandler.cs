using AutoMapper;
using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, HttpResult<UserResponse>>
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public GetUserByIdHandler(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<HttpResult<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<UserResponse>();
        var user = await _userService.GetUserByIdAsync(request.UserId);
        return result.WithValue(_mapper.Map<UserResponse>(user));
    }
}