using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.DeleteDoctorById;

public class DeleteDoctorByIdHandler : IRequestHandler<DeleteDoctorByIdCommand, HttpResult<Unit>>
{
    private readonly IDoctorRepository _doctorRepository;

    public DeleteDoctorByIdHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<HttpResult<Unit>> Handle(DeleteDoctorByIdCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var doctorDeleted = await _doctorRepository.DeleteByIdAsync(request.DoctorId);
        if (!doctorDeleted)
        {
            return result
                .WithError(new Error {Message = $"Doctor with id {request.DoctorId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(Unit.Value);
    }
}