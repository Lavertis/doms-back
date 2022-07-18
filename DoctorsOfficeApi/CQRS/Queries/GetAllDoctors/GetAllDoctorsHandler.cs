using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllDoctors;

public class GetAllDoctorsHandler : IRequestHandler<GetAllDoctorsQuery, IList<DoctorResponse>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetAllDoctorsHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IList<DoctorResponse>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctorResponses = await _doctorRepository.GetAll(a => a.AppUser)
            .Select(doctor => new DoctorResponse(doctor))
            .ToListAsync(cancellationToken: cancellationToken);
        return doctorResponses;
    }
}