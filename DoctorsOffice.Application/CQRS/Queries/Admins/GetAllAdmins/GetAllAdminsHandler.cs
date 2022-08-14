using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;

public class GetAllAdminsHandler : IRequestHandler<GetAllAdminsQuery, HttpResult<IEnumerable<AdminResponse>>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IMapper _mapper;

    public GetAllAdminsHandler(IAdminRepository adminRepository, IMapper mapper)
    {
        _adminRepository = adminRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<AdminResponse>>> Handle(
        GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<AdminResponse>>();
        var adminResponses = await _adminRepository
            .GetAll(admin => admin.AppUser)
            .Select(admin => _mapper.Map<AdminResponse>(admin))
            .ToListAsync(cancellationToken: cancellationToken);
        return result.WithValue(adminResponses);
    }
}