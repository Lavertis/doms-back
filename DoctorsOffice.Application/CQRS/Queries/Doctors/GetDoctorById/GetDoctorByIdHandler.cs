using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorById;

public class GetDoctorByIdHandler : IRequestHandler<GetDoctorByIdQuery, HttpResult<DoctorResponse>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorByIdHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<HttpResult<DoctorResponse>> Handle(GetDoctorByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<DoctorResponse>();
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, d => d.AppUser);
        if (doctor is null)
        {
            return result
                .WithError(new Error {Message = $"Doctor with id {request.DoctorId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(new DoctorResponse(doctor));
    }
}