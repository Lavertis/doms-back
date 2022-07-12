using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.PatientService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;

public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, PatientResponse>
{
    private readonly IPatientService _patientService;

    public GetPatientByIdHandler(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public async Task<PatientResponse> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetPatientByIdAsync(request.Id);

        return new PatientResponse(patient);
    }
}