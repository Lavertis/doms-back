using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetAllDoctors;

public class GetAllDoctorsHandler : IRequestHandler<GetAllDoctorsQuery, HttpResult<IEnumerable<DoctorResponse>>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetAllDoctorsHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<HttpResult<IEnumerable<DoctorResponse>>> Handle(GetAllDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<DoctorResponse>>();

        var doctorResponses = await _doctorRepository.GetAll(a => a.AppUser)
            .Select(doctor => new DoctorResponse(doctor))
            .ToListAsync(cancellationToken: cancellationToken);

        return result.WithValue(doctorResponses);
    }
}