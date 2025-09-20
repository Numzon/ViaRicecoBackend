using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

[UsedImplicitly]
#pragma warning disable CA1515
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public readonly TimeProvider TimeProviderMock = Substitute.For<TimeProvider>();

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("viariceco")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Start containers synchronously since ConfigureWebHost is called before InitializeAsync
        _dbContainer.StartAsync().GetAwaiter().GetResult();
        _redisContainer.StartAsync().GetAwaiter().GetResult();

        Environment.SetEnvironmentVariable("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:MessageBroker", "amqp://guest:guest@localhost:5672");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            
            TimeProviderMock.GetUtcNow().Returns(_ => DateTime.UtcNow);
            services.AddSingleton(TimeProviderMock);
        });
    }
    
    public async Task InitializeAsync()
    {
        // Containers are already started in ConfigureWebHost
        // Apply migrations to ensure database schema and seeding
        await EnsureDatabaseAsync();
    }

    private async Task EnsureDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        BudgetsDbContext dbContext = scope.ServiceProvider.GetRequiredService<BudgetsDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await base.DisposeAsync();
    }
}
#pragma warning restore CA1515
