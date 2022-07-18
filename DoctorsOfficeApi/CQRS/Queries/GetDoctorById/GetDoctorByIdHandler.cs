using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetDoctorById;

public class GetDoctorByIdHandler : IRequestHandler<GetDoctorByIdQuery, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorByIdHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<DoctorResponse> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.Id, d => d.AppUser);
        return new DoctorResponse(doctor);
    }
}