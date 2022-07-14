using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.DoctorService;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class DoctorServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly IDoctorService _doctorService;

    public DoctorServiceTests()
    {
        var inMemoryDbName = "InMemoryDb_" + DateTime.Now.ToFileTimeUtc();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        _doctorService = new DoctorService(_dbContext);
    }

    [Fact]
    public async Task GetDoctorById_DoctorWithSpecifiedIdExists_ReturnsDoctorWithSpecifiedId()
    {
        // arrange
        var doctor = new Doctor
        {
            AppUser = new AppUser { Id = "100" }
        };

        _dbContext.Doctors.Add(doctor);
        await _dbContext.SaveChangesAsync();

        // act
        var result = await _doctorService.GetDoctorByIdAsync(doctor.Id);

        // assert
        result.Should().BeEquivalentTo(doctor);
    }

    [Fact]
    public async Task GetDoctorById_DoctorWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange

        // act
        var action = async () => await _doctorService.GetDoctorByIdAsync("");

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}