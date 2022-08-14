using DoctorsOffice.API.Swagger;
using DoctorsOffice.Application.AutoMapper;
using DoctorsOffice.Application.CQRS;
using DoctorsOffice.Application.Middleware;
using DoctorsOffice.Application.Services;
using DoctorsOffice.Application.Validation;
using DoctorsOffice.Infrastructure;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Database;
using DoctorsOffice.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddMiddleware();
builder.Services.AddServices();
builder.Services.AddAutoMapper();
builder.Services.AddMediatR();
builder.Services.AddFluentValidators();
builder.Services.AddRepositories();

string GetHerokuConnectionString()
{
    var connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (connectionUrl is null)
        return string.Empty;

    var databaseUri = new Uri(connectionUrl);
    var db = databaseUri.LocalPath.TrimStart('/');
    var userInfo = databaseUri.UserInfo.Split(':', StringSplitOptions.RemoveEmptyEntries);
    return
        $"UserID={userInfo[0]};Password={userInfo[1]};Host={databaseUri.Host};Port={databaseUri.Port};Database={db};" +
        "Pooling=true;SSLMode=Require;TrustServerCertificate=True;";
}

var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
var connectionString = isDevelopment ? builder.Configuration.GetConnectionString("AppDb") : GetHerokuConnectionString();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));


const string corsPolicy = "DefaultPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy,
        policyBuilder =>
        {
            policyBuilder
                .WithOrigins(
                    "http://localhost:3000",
                    "https://localhost:3000",
                    "https://doms-back.herokuapp.com"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicy);

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}