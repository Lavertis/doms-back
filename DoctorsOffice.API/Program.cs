using DoctorsOffice.API.Cors;
using DoctorsOffice.API.Swagger;
using DoctorsOffice.Application.CQRS;
using DoctorsOffice.Application.Middleware;
using DoctorsOffice.Application.Services;
using DoctorsOffice.Application.Services.Chat;
using DoctorsOffice.Application.Validation;
using DoctorsOffice.Application.Workers;
using DoctorsOffice.DocumentGenerator;
using DoctorsOffice.Infrastructure.AutoMapper;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Database;
using DoctorsOffice.Infrastructure.Identity;
using DoctorsOffice.Infrastructure.Repositories;
using DoctorsOffice.SendGrid;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddWorkers();
builder.Services.AddControllers();
builder.Services.AddSwaggerModule();
builder.Services.AddServiceModule();
builder.Services.AddMediatorModule();
builder.Services.AddRepositoryModule();
builder.Services.AddMiddlewareModule();
builder.Services.AddAutoMapperModule();
builder.Services.AddDocumentGenerator();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFluentValidationModule();
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddDatabaseModule(builder.Configuration);
builder.Services.AddAppSettingsModule(builder.Configuration);
builder.Services.AddSendGrid(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    DatabaseUtils.ApplyMigrations(app.Services);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCorsModule(builder.Configuration.GetSection("Cors").Get<CorsSettings>());
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/api/chat");
app.Run();

public partial class Program
{
}