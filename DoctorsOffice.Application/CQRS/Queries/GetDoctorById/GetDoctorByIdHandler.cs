using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetDoctorById;

public class GetDoctorByIdHandler : IRequestHandler<GetDoctorByIdQuery, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorByIdHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<DoctorResponse> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, d => d.AppUser);
        return new DoctorResponse(doctor);
    }
}