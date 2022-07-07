using System.Reflection;
using System.Text;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Middleware;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Services.AppointmentService;
using DoctorsOfficeApi.Services.AuthService;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.UserService;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();

    // Set the comments path for the Swagger JSON and UI.
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };
        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            return new[] { controllerActionDescriptor.ControllerName };

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });
    options.DocInclusionPredicate((_, _) => true);
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("AppSettings:JwtSecretKey").Value);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            // Timezone problem workaround
            LifetimeValidator = (notBefore, expires, _, _) =>
            {
                if (notBefore is not null)
                    return notBefore.Value.AddMinutes(-5) <= DateTime.UtcNow && expires >= DateTime.UtcNow;
                return expires >= DateTime.UtcNow;
            }
        };
    });


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb")!)
);

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password = new PasswordOptions
        {
            RequireDigit = false,
            RequiredLength = 0,
            RequireLowercase = false,
            RequireUppercase = false,
            RequiredUniqueChars = 0,
            RequireNonAlphanumeric = false,
        };
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddScoped<ExceptionHandlerMiddleware>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddFluentValidation();
builder.Services.AddTransient<IValidator<AuthenticateRequest>, AuthenticateRequestValidator>();
builder.Services.AddTransient<IValidator<CreateAppointmentRequest>, CreateAppointmentRequestValidator>();
builder.Services.AddTransient<IValidator<UpdateAppointmentRequest>, UpdateAppointmentRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
};