using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, HttpResult<PagedResponse<UserResponse>>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IMapper _mapper;

    public GetAllUsersHandler(AppUserManager appUserManager, IMapper mapper)
    {
        _appUserManager = appUserManager;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<UserResponse>>> Handle(GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var userResponsesQueryable = _appUserManager.Users
            .Select(user => _mapper.Map<UserResponse>(user));

        return Task.FromResult(PaginationUtils.CreatePagedHttpResult(userResponsesQueryable, request.PaginationFilter));
    }
}