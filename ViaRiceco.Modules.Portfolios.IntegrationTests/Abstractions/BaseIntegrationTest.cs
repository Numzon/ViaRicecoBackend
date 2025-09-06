using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.IntegrationTests.Abstractions;


[Collection(nameof(IntegrationTestCollection))]
#pragma warning disable CA1515
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();
    private readonly IServiceScope _scope;
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly ISender Sender;
    protected readonly PortfoliosDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Factory = factory;
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<PortfoliosDbContext>();
    }
    
    protected async Task CleanDatabaseAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync(
            sql: """
                 -- Portfolio entities cleanup will go here
                 """);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _scope.Dispose();
    }
}
#pragma warning restore CA1515
