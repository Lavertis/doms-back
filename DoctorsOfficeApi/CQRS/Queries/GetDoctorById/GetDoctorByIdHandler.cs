using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.DoctorService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetDoctorById;

public class GetDoctorByIdHandler : IRequestHandler<GetDoctorByIdQuery, DoctorResponse>
{
    private readonly IDoctorService _doctorService;

    public GetDoctorByIdHandler(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task<DoctorResponse> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(request.Id);
        return new DoctorResponse(doctor);
    }
}