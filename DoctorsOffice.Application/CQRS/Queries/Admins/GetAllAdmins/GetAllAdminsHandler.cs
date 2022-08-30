using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;

public class GetAllAdminsHandler : IRequestHandler<GetAllAdminsQuery, HttpResult<PagedResponse<AdminResponse>>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IMapper _mapper;

    public GetAllAdminsHandler(IAdminRepository adminRepository, IMapper mapper)
    {
        _adminRepository = adminRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<AdminResponse>>> Handle(
        GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var adminResponsesQueryable = _adminRepository.GetAll()
            .Include(admin => admin.AppUser)
            .Select(admin => _mapper.Map<AdminResponse>(admin));

        return Task.FromResult(PaginationUtils.CreatePagedHttpResult(
            adminResponsesQueryable,
            request.PaginationFilter
        ));
    }
}