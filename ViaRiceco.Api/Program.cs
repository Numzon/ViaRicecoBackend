using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using ViaRiceco.Api.Enumeration;
using ViaRiceco.Api.Middlewares;
using ViaRiceco.Common.Infrastructure.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();

//throw if null
string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Database);
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow(ConnectionStrings.Cache);

//change to exceptions
builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString!)
    .AddRedis(redisConnectionString!);

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

await app.RunAsync();
