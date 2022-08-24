using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorById;

public class GetDoctorByIdHandler : IRequestHandler<GetDoctorByIdQuery, HttpResult<DoctorResponse>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;

    public GetDoctorByIdHandler(IDoctorRepository doctorRepository, IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<DoctorResponse>> Handle(GetDoctorByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<DoctorResponse>();
        var doctor = await _doctorRepository
            .GetAll()
            .Include(doctor => doctor.AppUser)
            .FirstOrDefaultAsync(doctor => doctor.Id == request.DoctorId, cancellationToken);

        if (doctor is null)
        {
            return result
                .WithError(new Error {Message = $"Doctor with id {request.DoctorId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var doctorResponse = _mapper.Map<DoctorResponse>(doctor);
        return result.WithValue(doctorResponse);
    }
}