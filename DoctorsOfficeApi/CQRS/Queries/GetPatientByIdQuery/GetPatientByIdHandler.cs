using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PatientRepository;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;

public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, PatientResponse>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientByIdHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientResponse> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, p => p.AppUser);

        return new PatientResponse(patient);
    }
}