using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using DoctorsOffice.Application.AutoMapper;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Infrastructure.Database;
using DoctorsOffice.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AuthenticateUrl = "/api/auth/authenticate";
    private const string TestAdminUserName = "testAdmin";
    private const string TestAdminPassword = "AdminPassword123!";
    private const string TestDoctorEmail = "testDoctor@email.com";
    private const string TestDoctorPassword = "DoctorPassword123!";
    private const string TestPatientEmail = "testPatient@email.com";
    private const string TestPatientPassword = "PatientPassword123!";

    private readonly WebApplicationFactory<Program> _factory;
    protected readonly IMapper Mapper = AutoMapperModule.CreateAutoMapper();
    private AppUserManager _appUserManager = null!;
    protected AppDbContext DbContext = null!;

    protected IntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IHostedService));

                var dbContextOptions = services.SingleOrDefault(service =>
                    service.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbContextOptions is not null)
                    services.Remove(dbContextOptions);

                var inMemoryDbName =
                    "InMemoryDb_" + Guid.NewGuid(); // workaround for concurrent integration tests using same db
                services.AddDbContext<AppDbContext>(options => { options.UseInMemoryDatabase(inMemoryDbName); });

                DbContext = services.BuildServiceProvider().GetService<AppDbContext>()!;
                _appUserManager = services.BuildServiceProvider().GetService<AppUserManager>()!;
            });
        });
    }

    protected void RefreshDbContext() // refreshes context for assertions
    {
        var scope = _factory.Services.GetService<IServiceScopeFactory>()!.CreateScope();
        var context = scope.ServiceProvider.GetService<AppDbContext>()!;
        DbContext = context;
    }

    protected async Task<HttpClient> GetHttpClientAsync()
    {
        var client = _factory.CreateClient();
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
        await SeedUsers();
        return client;
    }

    private async Task SeedUsers()
    {
        var testAdminUser = new AppUser
        {
            UserName = TestAdminUserName,
            Email = TestAdminUserName,
            FirstName = "testAdminFirstName",
            LastName = "testAdminLastName",
            EmailConfirmed = true
        };
        await CreateUserAsync(testAdminUser, TestAdminPassword, Roles.Admin);
        var testAdmin = new Admin {Id = testAdminUser.Id};
        await DbContext.Admins.AddAsync(testAdmin);

        var testDoctorUser = new AppUser
        {
            UserName = TestDoctorEmail,
            Email = TestDoctorEmail,
            FirstName = "testDoctorFirstName",
            LastName = "testDoctorLastName",
            EmailConfirmed = true
        };
        await CreateUserAsync(testDoctorUser, TestDoctorPassword, Roles.Doctor);
        var testDoctor = new Doctor {Id = testDoctorUser.Id};
        await DbContext.Doctors.AddAsync(testDoctor);

        var testPatientUser = new AppUser
        {
            UserName = TestPatientEmail,
            Email = TestPatientEmail,
            PhoneNumber = "1234567890",
            FirstName = "testPatientFirstName",
            LastName = "testPatientLastName",
            EmailConfirmed = true
        };
        await CreateUserAsync(testPatientUser, TestPatientPassword, Roles.Patient);
        var testPatient = new Patient
        {
            Id = testPatientUser.Id,
            NationalId = "1234567890",
            Address = "testPatientAddress"
        };
        await DbContext.Patients.AddAsync(testPatient);

        await DbContext.SaveChangesAsync();
        RefreshDbContext();
    }

    private async Task CreateUserAsync(AppUser appUser, string password, string roleName)
    {
        var createUserIdentityResult = await _appUserManager.CreateAsync(appUser, password);
        if (!createUserIdentityResult.Succeeded)
            throw new Exception("UserManager could not create user");
        var addToRoleIdentityResult = await _appUserManager.AddToRoleAsync(appUser, roleName);
        if (!addToRoleIdentityResult.Succeeded)
            throw new Exception("UserManager could not add user to role");
    }

    protected static async Task<Guid> AuthenticateAsRoleAsync(HttpClient client, string roleName)
    {
        return roleName switch
        {
            Roles.Admin => await AuthenticateAsAdminAsync(client),
            Roles.Doctor => await AuthenticateAsDoctorAsync(client),
            Roles.Patient => await AuthenticateAsPatientAsync(client),
            _ => throw new ArgumentException($"Unknown role name: {roleName}")
        };
    }

    protected static async Task<Guid> AuthenticateAsAdminAsync(HttpClient client)
    {
        return await AuthenticateAsAsync(client, TestAdminUserName, TestAdminPassword);
    }

    protected static async Task<Guid> AuthenticateAsDoctorAsync(HttpClient client)
    {
        return await AuthenticateAsAsync(client, TestDoctorEmail, TestDoctorPassword);
    }

    protected static async Task<Guid> AuthenticateAsPatientAsync(HttpClient client)
    {
        return await AuthenticateAsAsync(client, TestPatientEmail, TestPatientPassword);
    }

    protected static async Task<Guid> AuthenticateAsAsync(HttpClient client, string userName, string password)
    {
        var result = await client.PostAsJsonAsync(AuthenticateUrl, new AuthenticateRequest
        {
            Login = userName,
            Password = password
        });

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception("Failed to authenticate as user");
        }

        var jwtToken = (await result.Content.ReadAsAsync<AuthenticateResponse>()).JwtToken;

        client.DefaultRequestHeaders.Add("Authorization", $"bearer {jwtToken}");

        var handler = new JwtSecurityTokenHandler();
        var jwtObject = handler.ReadJwtToken(jwtToken);
        var userId = jwtObject.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub)!.Value;
        return Guid.Parse(userId);
    }
}