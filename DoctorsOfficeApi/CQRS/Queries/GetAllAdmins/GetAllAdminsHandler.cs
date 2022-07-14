using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllAdmins;

public class GetAllAdminsHandler : IRequestHandler<GetAllAdminsQuery, IList<AdminResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetAllAdminsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<AdminResponse>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var adminResponses = await _dbContext.Admins.Select(a => new AdminResponse(a))
            .ToListAsync(cancellationToken: cancellationToken);
        return adminResponses;
    }
}