using FastEndpoints.Swagger;
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
using ViaRiceco.Modules.Portfolios.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddPresentation([
    ViaRiceco.Modules.Accounting.Presentation.AssemblyReference.Assembly,
    ViaRiceco.Modules.Portfolios.Presentation.AssemblyReference.Assembly,
]);

builder.Services.AddApplication([
    ViaRiceco.Modules.Accounting.Application.AssemblyReference.Assembly,
    ViaRiceco.Modules.Portfolios.Application.AssemblyReference.Assembly,
]);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Database);
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Cache);

builder.Services.AddInfrastructure(DiagnosticsConfig.ServiceName,
    [
    ],
    databaseConnectionString, redisConnectionString);

builder.Services.SwaggerDocument(o => 
{
    o.AutoTagPathSegmentIndex = -1; // Disable automatic tagging from path segments
});

builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString);

builder.Configuration.AddModuleConfiguration([
    Modules.Accounting,
    Modules.Portfolios
]);

builder.Services.AddAccountingModule(builder.Configuration);
builder.Services.AddPortfoliosModule(builder.Configuration);

builder.Services.AddHttpContextAccessor();

WebApplication app = builder.Build();

app.MapFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();

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

#pragma warning disable CA1515
// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;
