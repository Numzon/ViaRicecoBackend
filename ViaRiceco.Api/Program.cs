using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using ViaRiceco.Api.Enumerations;
using ViaRiceco.Api.Extensions;
using ViaRiceco.Api.Middlewares;
using ViaRiceco.Common.Infrastructure.Configuration;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Common.Presentation;
using ViaRiceco.Modules.Accounting.Infrastructure;
using ViaRiceco.Modules.Accounting.Presentation;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddPresentation([
    ViaRiceco.Modules.Accounting.Presentation.AssemblyReference.Assembly,
]);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Database);
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Cache);

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

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

await app.RunAsync();
