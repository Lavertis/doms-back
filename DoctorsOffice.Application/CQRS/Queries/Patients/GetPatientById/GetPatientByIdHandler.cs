using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientById;

public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, HttpResult<PatientResponse>>
{
    private readonly IMapper _mapper;
    private readonly IPatientRepository _patientRepository;

    public GetPatientByIdHandler(IPatientRepository patientRepository, IMapper mapper)
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<PatientResponse>> Handle(GetPatientByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<PatientResponse>();
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, p => p.AppUser);
        if (patient is null)
        {
            return result
                .WithError(new Error {Message = $"Patient with id {request.PatientId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var patientResponse = _mapper.Map<PatientResponse>(patient);
        return result.WithValue(patientResponse);
    }
}