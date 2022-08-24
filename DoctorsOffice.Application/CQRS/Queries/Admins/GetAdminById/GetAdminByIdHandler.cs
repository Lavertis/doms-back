using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Admins.GetAdminById;

public class GetAdminByIdHandler : IRequestHandler<GetAdminByIdQuery, HttpResult<AdminResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IMapper _mapper;

    public GetAdminByIdHandler(IAdminRepository adminRepository, IMapper mapper)
    {
        _adminRepository = adminRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<AdminResponse>> Handle(GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AdminResponse>();
        var admin = await _adminRepository.GetAll()
            .Include(admin => admin.AppUser)
            .FirstOrDefaultAsync(admin => admin.Id == request.AdminId, cancellationToken);

        if (admin is null)
        {
            return result
                .WithError(new Error {Message = $"Admin with id {request.AdminId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var adminResponse = _mapper.Map<AdminResponse>(admin);
        return result.WithValue(adminResponse);
    }
}