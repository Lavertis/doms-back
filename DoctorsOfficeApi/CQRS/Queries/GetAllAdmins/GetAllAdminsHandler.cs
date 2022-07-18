using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AdminRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllAdmins;

public class GetAllAdminsHandler : IRequestHandler<GetAllAdminsQuery, IList<AdminResponse>>
{
    private readonly IAdminRepository _adminRepository;

    public GetAllAdminsHandler(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<IList<AdminResponse>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var adminResponses = await _adminRepository
            .GetAll(a => a.AppUser)
            .Select(a => new AdminResponse(a))
            .ToListAsync(cancellationToken: cancellationToken);
        return adminResponses;
    }
}