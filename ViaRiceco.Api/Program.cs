using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using ViaRiceco.Api.Enumerations;
using ViaRiceco.Api.Extensions;
using ViaRiceco.Api.Middlewares;
using ViaRiceco.Api.OpenTelemetry;
using ViaRiceco.Common.Application;
using ViaRiceco.Common.Infrastructure;
using ViaRiceco.Common.Infrastructure.Configuration;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Common.Presentation;
using ViaRiceco.Modules.Accounting.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddPresentation([
    ViaRiceco.Modules.Accounting.Presentation.AssemblyReference.Assembly,
]);

builder.Services.AddApplication([
    ViaRiceco.Modules.Accounting.Application.AssemblyReference.Assembly,
]);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Database);
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Cache);

builder.Services.AddInfrastructure(DiagnosticsConfig.ServiceName, databaseConnectionString, redisConnectionString);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString);

builder.Configuration.AddModuleConfiguration([
    Modules.Accounting
]);

builder.Services.AddAccountingModule(builder.Configuration);

WebApplication app = builder.Build();

app.MapFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.ApplyMigrations();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseLogContext();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseHttpsRedirection();

await app.RunAsync();
