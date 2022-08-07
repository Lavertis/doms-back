using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AdminRepository;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAdminById;

public class GetAdminByIdHandler : IRequestHandler<GetAdminByIdQuery, AdminResponse>
{
    private readonly IAdminRepository _adminRepository;

    public GetAdminByIdHandler(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<AdminResponse> Handle(GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        var admin = await _adminRepository.GetByIdAsync(request.AdminId, a => a.AppUser);
        return new AdminResponse(admin);
    }
}