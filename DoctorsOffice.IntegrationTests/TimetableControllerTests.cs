using System.Net;
using System.Text;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class TimetableControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/timetables";

    public TimetableControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAllTimetablesByDoctorIdDoctorAsync_DoctorIdExists_ReturnsAllTimetablesBelongingToDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);
        var otherDoctor = new Doctor {AppUser = new AppUser {FirstName = "", LastName = ""}};
        await DbContext.Doctors.AddAsync(otherDoctor);

        var authenticatedDoctorsTimetables = CreateTimetables(10, authenticatedDoctorId);
        await DbContext.Timetables.AddRangeAsync(authenticatedDoctorsTimetables);

        var otherDoctorsTimetables = CreateTimetables(10, otherDoctor.Id);
        await DbContext.Timetables.AddRangeAsync(otherDoctorsTimetables);

        await DbContext.SaveChangesAsync();

        var expectedResponse = authenticatedDoctorsTimetables.Select(t => Mapper.Map<TimetableResponse>(t));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/{authenticatedDoctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<TimetableResponse>>();

        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAllTimetablesByDoctorIdDoctorAsync_FiltersProvided_ReturnsAllAppointmentsMatchingConstraints()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(10, authenticatedDoctorId);
        await DbContext.Timetables.AddRangeAsync(timetables);
        await DbContext.SaveChangesAsync();

        var startDateTime = DateTime.UtcNow.Add(5.Hours());
        var endDateTime = DateTime.UtcNow.Add(20.Hours());

        var expectedResponse = timetables
            .Where(t => t.DoctorId == authenticatedDoctorId)
            .Where(t => t.StartDateTime >= startDateTime)
            .Where(t => t.EndDateTime <= endDateTime)
            .Select(t => Mapper.Map<TimetableResponse>(t));

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("startDateTime", startDateTime.ToString("o"));
        queryString.Add("endDateTime", endDateTime.ToString("o"));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/{authenticatedDoctorId}?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<IEnumerable<TimetableResponse>>();

        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAllTimetablesByDoctorIdDoctorAsync_DoctorWithSpecifiedIdIdDoesntExist_ReturnEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var nonExistingDoctorId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/{nonExistingDoctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<TimetableResponse>>();

        responseContent.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTimetablesByDoctorIdDoctorAsync_DoctorWithSpecifiedIdDoesntHaveTimetables_ReturnEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/{authenticatedDoctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<TimetableResponse>>();

        responseContent.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Roles.Doctor)]
    [InlineData(Roles.Patient)]
    public async Task GetAllTimetablesByDoctorIdDoctorAsync_AuthenticatedUserIsNotAdmin_ReturnsOk(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);
        var doctorId = DbContext.Doctors.First().Id;

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/{doctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllTimetablesByDoctorIdDoctorAsync_AuthenticatedUserIsAdmin_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);
        var doctorId = DbContext.Doctors.First().Id;

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/{doctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllTimetablesByDoctorIdDoctorAsync_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var doctorId = DbContext.Doctors.First().Id;

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/{doctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTimetablesForAuthenticatedDoctorAsync_ValidRequest_CreatesTimetables()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var requests = new CreateTimetableRequestList
        {
            new() {StartDateTime = DateTime.UtcNow.Add(5.Minutes()), EndDateTime = DateTime.UtcNow.Add(15.Minutes())},
            new() {StartDateTime = DateTime.UtcNow.Add(20.Minutes()), EndDateTime = DateTime.UtcNow.Add(30.Minutes())}
        };

        var expectedResponse = requests.Select(t => new TimetableResponse
        {
            DoctorId = authenticatedDoctorId,
            StartDateTime = t.StartDateTime,
            EndDateTime = t.EndDateTime
        });

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current/batch", requests);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RefreshDbContext();

        foreach (var expectedResponseItem in expectedResponse)
        {
            DbContext.Timetables.Should().Contain(
                t =>
                    t.StartDateTime == expectedResponseItem.StartDateTime &&
                    t.EndDateTime == expectedResponseItem.EndDateTime &&
                    t.DoctorId == expectedResponseItem.DoctorId
            );
        }
    }

    [Fact]
    public async Task CreateTimetablesForAuthenticatedDoctorAsync_EndDateBeforeStartDateInOneRequest_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var requests = new CreateTimetableRequestList
        {
            new() {StartDateTime = DateTime.UtcNow.Add(10.Minutes()), EndDateTime = DateTime.UtcNow.Add(5.Minutes())},
            new() {StartDateTime = DateTime.UtcNow.Add(20.Minutes()), EndDateTime = DateTime.UtcNow.Add(30.Minutes())}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current/batch", requests);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTimetablesForAuthenticatedDoctorAsync_TimetableOverlapsWithExistingOne_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        DbContext.Timetables.Add(new Timetable
        {
            DoctorId = authenticatedDoctorId,
            StartDateTime = DateTime.UtcNow.Add(5.Minutes()),
            EndDateTime = DateTime.UtcNow.Add(15.Minutes())
        });
        await DbContext.SaveChangesAsync();

        var requests = new List<CreateTimetableRequest>
        {
            new() {StartDateTime = DateTime.UtcNow.Add(5.Minutes()), EndDateTime = DateTime.UtcNow.Add(10.Minutes())},
            new() {StartDateTime = DateTime.UtcNow.Add(20.Minutes()), EndDateTime = DateTime.UtcNow.Add(30.Minutes())}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current/batch", requests);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task
        CreateTimetablesForAuthenticatedDoctorAsync_TwoTimetablesInRequestAreOverlapping_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var requests = new List<CreateTimetableRequest>
        {
            new() {StartDateTime = DateTime.UtcNow.Add(5.Minutes()), EndDateTime = DateTime.UtcNow.Add(15.Minutes())},
            new() {StartDateTime = DateTime.UtcNow.Add(10.Minutes()), EndDateTime = DateTime.UtcNow.Add(20.Minutes())}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current/batch", requests);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTimetablesForAuthenticatedDoctorAsync_OneOfTheDatesInRequestIsInThePast_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var requests = new List<CreateTimetableRequest>
        {
            new()
            {
                StartDateTime = DateTime.UtcNow.Subtract(5.Minutes()), EndDateTime = DateTime.UtcNow.Add(10.Minutes())
            },
            new() {StartDateTime = DateTime.UtcNow.Add(5.Minutes()), EndDateTime = DateTime.UtcNow.Add(20.Minutes())}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current/batch", requests);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTimetablesForAuthenticatedDoctorAsync_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var requests = new List<CreateTimetableRequest>
        {
            new() {StartDateTime = DateTime.UtcNow.Add(5.Minutes()), EndDateTime = DateTime.UtcNow.Add(15.Minutes())},
            new() {StartDateTime = DateTime.UtcNow.Add(20.Minutes()), EndDateTime = DateTime.UtcNow.Add(30.Minutes())}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current/batch", requests);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTimetablesForAuthenticatedDoctorAsync_AuthenticatedUserInNotDoctor_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var requests = new List<CreateTimetableRequest>
        {
            new() {StartDateTime = DateTime.UtcNow.Add(5.Minutes()), EndDateTime = DateTime.UtcNow.Add(15.Minutes())},
            new() {StartDateTime = DateTime.UtcNow.Add(20.Minutes()), EndDateTime = DateTime.UtcNow.Add(30.Minutes())}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current/batch", requests);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateTimetablesAsync_ValidRequest_UpdatesTimetables()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(5, authenticatedDoctorId);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new UpdateTimetableBatchRequestList
        {
            new()
            {
                Id = timetables[1].Id,
                StartDateTime = timetables[1].StartDateTime,
                EndDateTime = timetables[1].EndDateTime.Add(10.Minutes())
            },
            new()
            {
                Id = timetables[2].Id,
                StartDateTime = timetables[2].StartDateTime.Add(10.Minutes()),
                EndDateTime = timetables[2].EndDateTime
            }
        };

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/batch", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<IList<TimetableResponse>>();
        RefreshDbContext();

        foreach (var request in requests)
        {
            responseContent.Should().ContainEquivalentOf(Mapper.Map<UpdateTimetableBatchRequest>(request));
        }
    }

    [Fact]
    public async Task UpdateTimetablesAsync_EndDateBeforeStartDate_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(5, authenticatedDoctorId);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<UpdateTimetableBatchRequest>
        {
            new() {Id = timetables[1].Id, EndDateTime = timetables[1].EndDateTime.Add(10.Minutes())},
            new()
            {
                Id = timetables[2].Id,
                StartDateTime = timetables[2].StartDateTime.Add(10.Minutes()),
                EndDateTime = timetables[2].StartDateTime
            }
        };

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/batch", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTimetablesAsync_UpdatedTimetablesAreOverlapping_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(5, authenticatedDoctorId);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<UpdateTimetableBatchRequest>
        {
            new()
            {
                Id = timetables[1].Id,
                StartDateTime = DateTime.UtcNow.Add(5.Minutes()),
                EndDateTime = DateTime.UtcNow.Add(15.Minutes())
            },
            new()
            {
                Id = timetables[2].Id,
                StartDateTime = DateTime.UtcNow.Add(10.Minutes()),
                EndDateTime = DateTime.UtcNow.Add(20.Minutes())
            }
        };

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/batch", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTimetablesAsync_UpdatedTimetableOverlapsWithExistingOne_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(5, authenticatedDoctorId);
        DbContext.Timetables.AddRange(timetables);

        DbContext.Timetables.Add(new Timetable
        {
            DoctorId = authenticatedDoctorId,
            StartDateTime = DateTime.UtcNow.Add(5.Minutes()),
            EndDateTime = DateTime.UtcNow.Add(15.Minutes())
        });
        await DbContext.SaveChangesAsync();

        var requests = new List<UpdateTimetableBatchRequest>
        {
            new()
            {
                Id = timetables[1].Id, StartDateTime = DateTime.UtcNow.Add(5.Minutes()),
                EndDateTime = DateTime.UtcNow.Add(10.Minutes())
            },
            new()
            {
                Id = timetables[2].Id, StartDateTime = DateTime.UtcNow.Add(20.Minutes()),
                EndDateTime = DateTime.UtcNow.Add(30.Minutes())
            }
        };

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/batch", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTimetablesAsync_OneTimetableDoesntExist_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(5, authenticatedDoctorId);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<UpdateTimetableBatchRequest>
        {
            new() {Id = timetables[1].Id, EndDateTime = timetables[1].EndDateTime.Add(10.Minutes())},
            new() {Id = Guid.NewGuid(), StartDateTime = timetables[2].StartDateTime.Add(10.Minutes())}
        };

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/batch", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTimetablesAsync_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var timetables = CreateTimetables(5);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<UpdateTimetableBatchRequest>
        {
            new() {Id = timetables[1].Id, EndDateTime = timetables[1].EndDateTime.Add(10.Minutes())},
            new() {Id = timetables[2].Id, StartDateTime = timetables[2].StartDateTime.Add(10.Minutes())}
        };

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/batch", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTimetablesAsync_AuthenticatedUserInNotDoctor_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var timetables = CreateTimetables(5);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<UpdateTimetableBatchRequest>
        {
            new() {Id = timetables[1].Id, EndDateTime = timetables[1].EndDateTime.Add(10.Minutes())},
            new() {Id = timetables[2].Id, StartDateTime = timetables[2].StartDateTime.Add(10.Minutes())}
        };

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/batch", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTimetablesByIdAsync_ValidRequest_DeletesTimetables()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(5, authenticatedDoctorId);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<Guid> {timetables[1].Id, timetables[3].Id};

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{UrlPrefix}/batch", UriKind.Relative),
            Content = content
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        foreach (var idToDelete in requests)
        {
            DbContext.Timetables.Should().NotContain(t => t.Id == idToDelete);
        }
    }

    [Fact]
    public async Task DeleteTimetablesByIdAsync_OneTimetableDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var timetables = CreateTimetables(5, authenticatedDoctorId);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<Guid> {timetables[1].Id, Guid.NewGuid()};

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{UrlPrefix}/batch", UriKind.Relative),
            Content = content
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTimetablesByIdAsync_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var timetables = CreateTimetables(5);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<Guid> {timetables[1].Id, timetables[3].Id};

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{UrlPrefix}/batch", UriKind.Relative),
            Content = content
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTimetablesByIdAsync_AuthenticatedUserIsNotDoctor_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var timetables = CreateTimetables(5);
        DbContext.Timetables.AddRange(timetables);
        await DbContext.SaveChangesAsync();

        var requests = new List<Guid> {timetables[1].Id, timetables[3].Id};

        var serializedContent = JsonConvert.SerializeObject(requests);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{UrlPrefix}/batch", UriKind.Relative),
            Content = content
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private IList<Timetable> CreateTimetables(int count, Guid? doctorId = null)
    {
        doctorId ??= DbContext.Doctors.First().Id;

        var timetables = new List<Timetable>();
        for (var i = 0; i < count; i++)
        {
            timetables.Add(new Timetable
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId.Value,
                StartDateTime = DateTime.UtcNow.Add((5 * i).Hours()),
                EndDateTime = DateTime.UtcNow.Add((5 * i + 4).Hours())
            });
        }

        return timetables;
    }
}