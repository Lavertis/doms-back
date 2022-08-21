using System.IdentityModel.Tokens.Jwt;
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
    private const string TestDoctorUserName = "testDoctor";
    private const string TestDoctorPassword = "DoctorPassword123!";
    private const string TestPatientUserName = "testPatient";
    private const string TestPatientPassword = "PatientPassword123!";

    private readonly WebApplicationFactory<Program> _factory;
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
            UserName = TestAdminUserName
        };
        await CreateUserAsync(testAdminUser, TestAdminPassword, RoleTypes.Admin);
        var testAdmin = new Admin {Id = testAdminUser.Id,};
        await DbContext.Admins.AddAsync(testAdmin);

        var testDoctorUser = new AppUser
        {
            UserName = TestDoctorUserName
        };
        await CreateUserAsync(testDoctorUser, TestDoctorPassword, RoleTypes.Doctor);
        var testDoctor = new Doctor {Id = testDoctorUser.Id,};
        await DbContext.Doctors.AddAsync(testDoctor);

        var testPatientUser = new AppUser
        {
            UserName = TestPatientUserName
        };
        await CreateUserAsync(testPatientUser, TestPatientPassword, RoleTypes.Patient);
        var testPatient = new Patient
        {
            Id = testPatientUser.Id,
            FirstName = "testPatientFirstName",
            LastName = "testPatientLastName",
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
            RoleTypes.Admin => await AuthenticateAsAdminAsync(client),
            RoleTypes.Doctor => await AuthenticateAsDoctorAsync(client),
            RoleTypes.Patient => await AuthenticateAsPatientAsync(client),
            _ => throw new ArgumentException($"Unknown role name: {roleName}")
        };
    }

    protected static async Task<Guid> AuthenticateAsAdminAsync(HttpClient client)
    {
        return await AuthenticateAsAsync(client, TestAdminUserName, TestAdminPassword);
    }

    protected static async Task<Guid> AuthenticateAsDoctorAsync(HttpClient client)
    {
        return await AuthenticateAsAsync(client, TestDoctorUserName, TestDoctorPassword);
    }

    protected static async Task<Guid> AuthenticateAsPatientAsync(HttpClient client)
    {
        return await AuthenticateAsAsync(client, TestPatientUserName, TestPatientPassword);
    }

    protected static async Task<Guid> AuthenticateAsAsync(HttpClient client, string userName, string password)
    {
        var result = await client.PostAsJsonAsync(AuthenticateUrl, new AuthenticateRequest
        {
            UserName = userName,
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