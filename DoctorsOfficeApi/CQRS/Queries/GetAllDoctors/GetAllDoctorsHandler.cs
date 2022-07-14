using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllDoctors;

public class GetAllDoctorsHandler : IRequestHandler<GetAllDoctorsQuery, IList<DoctorResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetAllDoctorsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<DoctorResponse>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctorResponses = await _dbContext.Doctors
            .Select(doctor => new DoctorResponse(doctor))
            .ToListAsync(cancellationToken: cancellationToken);
        return doctorResponses;
    }
}