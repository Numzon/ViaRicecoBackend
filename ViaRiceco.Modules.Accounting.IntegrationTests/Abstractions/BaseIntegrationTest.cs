using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;


[Collection(nameof(IntegrationTestCollection))]
#pragma warning disable CA1515
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();
    private readonly IServiceScope _scope;
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly ISender Sender;
    protected readonly AccountingDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Factory = factory;
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
    }
    
    protected async Task CleanDatabaseAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync(
            sql: """
                 DELETE FROM accounting.income;
                 DELETE FROM accounting.settlement_periods;
                 DELETE FROM accounting.tax;
                 DELETE FROM accounting.tax_types;
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
