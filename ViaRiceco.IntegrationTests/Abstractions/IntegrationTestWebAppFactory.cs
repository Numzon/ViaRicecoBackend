using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ViaRiceco.IntegrationTests.Abstractions;

[UsedImplicitly]
#pragma warning disable CA1515
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("evently")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
        
        // Disable Quartz background jobs during integration testing to prevent DbContext concurrency issues
        builder.ConfigureServices(services =>
        {
            // Remove all Quartz-related services
            var quartzServices = services
                .Where(descriptor => 
                    descriptor.ServiceType.FullName?.Contains("Quartz") == true ||
                    descriptor.ImplementationType?.FullName?.Contains("Quartz") == true)
                .ToList();

            foreach (ServiceDescriptor service in quartzServices)
            {
                services.Remove(service);
            }
            
            // Also remove any hosted services that could be background processors
            var hostedServices = services
                .Where(descriptor => 
                    descriptor.ServiceType.Name.Contains("IHostedService") ||
                    descriptor.ImplementationType?.Name.Contains("HostedService") == true)
                .ToList();

            foreach (ServiceDescriptor service in hostedServices)
            {
                services.Remove(service);
            }
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}
#pragma warning restore CA1515
