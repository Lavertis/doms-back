using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Services.DoctorService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;

public class DeleteDoctorByIdHandler : IRequestHandler<DeleteDoctorByIdCommand, Unit>
{
    private readonly AppDbContext _dbContext;
    private readonly IDoctorService _doctorService;

    public DeleteDoctorByIdHandler(AppDbContext dbContext, IDoctorService doctorService)
    {
        _dbContext = dbContext;
        _doctorService = doctorService;
    }

    public async Task<Unit> Handle(DeleteDoctorByIdCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(request.Id);
        _dbContext.Doctors.Remove(doctor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}