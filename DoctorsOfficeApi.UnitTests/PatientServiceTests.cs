using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.PatientService;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class PatientServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly PatientService _patientService;

    public PatientServiceTests()
    {
        var inMemoryDbName = "InMemoryDb_" + Guid.NewGuid();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _dbContext = new AppDbContext(dbContextOptions);

        _patientService = new PatientService(_dbContext);
    }

    [Fact]
    public async Task GetPatientById_PatientExists_ReturnsPatient()
    {
        // arrange
        var patient = new Patient
        {
            FirstName = "firstName",
            LastName = "lastName",
            Address = "address",
            AppUser = new AppUser { Id = Guid.NewGuid() }
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // act
        var result = await _patientService.GetPatientByIdAsync(patient.Id);

        // assert
        result.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetPatientByIdAsync_PatientDoesntExist_ThrowsNotFoundException()
    {
        // arrange

        var nenExistingPatientId = Guid.NewGuid();

        // act
        var action = async () => await _patientService.GetPatientByIdAsync(nenExistingPatientId);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}