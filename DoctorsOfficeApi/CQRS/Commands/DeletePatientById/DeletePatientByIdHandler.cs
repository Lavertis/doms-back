using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Services.PatientService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeletePatientById;

public class DeletePatientByIdHandler : IRequestHandler<DeletePatientByIdCommand, Unit>
{
    private readonly AppDbContext _dbContext;
    private readonly IPatientService _patientService;

    public DeletePatientByIdHandler(AppDbContext dbContext, IPatientService patientService)
    {
        _dbContext = dbContext;
        _patientService = patientService;
    }

    public async Task<Unit> Handle(DeletePatientByIdCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetPatientByIdAsync(request.Id);

        _dbContext.Patients.Remove(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}