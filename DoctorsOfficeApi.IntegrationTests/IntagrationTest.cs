using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DoctorsOfficeApi.IntegrationTests;

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

                var inMemoryDbName = "InMemoryDb_" + Guid.NewGuid(); // workaround for concurrent integration tests using same db
                services.AddDbContext<AppDbContext>(options => { options.UseInMemoryDatabase(inMemoryDbName); });

                DbContext = services.BuildServiceProvider().GetService<AppDbContext>()!;
            });
        });
    }

    protected void RefreshDbContext() // refreshes context for assertions
    {
        var scope = _factory.Services.GetService<IServiceScopeFactory>()!.CreateScope();
        var context = scope.ServiceProvider.GetService<AppDbContext>()!;
        DbContext = context;
    }

    protected HttpClient GetHttpClient()
    {
        var client = _factory.CreateClient();
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
        SeedUsers(DbContext);
        return client;
    }

    private void SeedUsers(AppDbContext dbContext)
    {
        var hasher = new PasswordHasher<AppUser>();

        var testAdminUserId = Guid.NewGuid();
        var testAdminUser = new AppUser
        {
            Id = testAdminUserId,
            UserName = TestAdminUserName,
            NormalizedUserName = TestAdminUserName.ToUpper(),
            PasswordHash = hasher.HashPassword(null!, TestAdminPassword),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        var testAdmin = new Admin { AppUser = testAdminUser };
        dbContext.Admins.Add(testAdmin);
        var adminRoleId = dbContext.Roles.FirstOrDefault(r => r.Name == RoleTypes.Admin)!.Id;
        dbContext.IdentityUserRole.Add(new IdentityUserRole<Guid>
        {
            UserId = testAdminUserId,
            RoleId = adminRoleId,
        });

        var testDoctorUserId = Guid.NewGuid();
        var testDoctorUser = new AppUser
        {
            Id = testDoctorUserId,
            UserName = TestDoctorUserName,
            NormalizedUserName = TestDoctorUserName.ToUpper(),
            PasswordHash = hasher.HashPassword(null!, TestDoctorPassword),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        var testDoctor = new Doctor { AppUser = testDoctorUser };
        dbContext.Doctors.Add(testDoctor);
        var doctorRoleId = dbContext.Roles.FirstOrDefault(r => r.Name == RoleTypes.Doctor)!.Id;
        dbContext.IdentityUserRole.Add(new IdentityUserRole<Guid>
        {
            UserId = testDoctorUserId,
            RoleId = doctorRoleId,
        });

        var patientUserId = Guid.NewGuid();
        var testPatientUser = new AppUser
        {
            Id = patientUserId,
            UserName = TestPatientUserName,
            NormalizedUserName = TestPatientUserName.ToUpper(),
            PasswordHash = hasher.HashPassword(null!, TestPatientPassword),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        var testPatient = new Patient
        {
            FirstName = "testPatientFirstName",
            LastName = "testPatientLastName",
            Address = "testPatientAddress",
            AppUser = testPatientUser
        };
        dbContext.Patients.Add(testPatient);
        var patientRoleId = dbContext.Roles.FirstOrDefault(r => r.Name == RoleTypes.Patient)!.Id;
        dbContext.IdentityUserRole.Add(new IdentityUserRole<Guid>
        {
            UserId = patientUserId,
            RoleId = patientRoleId,
        });

        dbContext.SaveChanges();
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
            throw new AppException("Failed to authenticate as user");
        }

        var jwtToken = (await result.Content.ReadAsAsync<AuthenticateResponse>()).JwtToken;

        client.DefaultRequestHeaders.Add("Authorization", $"bearer {jwtToken}");

        var handler = new JwtSecurityTokenHandler();
        var jwtObject = handler.ReadJwtToken(jwtToken);
        var userId = jwtObject.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)!.Value;
        return Guid.Parse(userId);
    }
}