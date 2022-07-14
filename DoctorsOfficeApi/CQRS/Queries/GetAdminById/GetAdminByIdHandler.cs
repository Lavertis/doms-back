using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AdminService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAdminById;

public class GetAdminByIdHandler : IRequestHandler<GetAdminByIdQuery, AdminResponse>
{
    private readonly IAdminService _adminService;

    public GetAdminByIdHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<AdminResponse> Handle(GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        var admin = await _adminService.GetAdminByIdAsync(request.Id);
        return new AdminResponse(admin);
    }
}