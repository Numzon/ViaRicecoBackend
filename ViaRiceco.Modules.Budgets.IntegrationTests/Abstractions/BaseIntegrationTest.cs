using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
#pragma warning disable CA1515
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();
    private readonly IServiceScope _scope;
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly ISender Sender;
    protected readonly BudgetsDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Factory = factory;
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<BudgetsDbContext>();
    }
    
    protected async Task CleanDatabaseAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync(
            sql: """
                 DELETE FROM budgets.expenses;
                 DELETE FROM budgets.expense_types WHERE is_system_defined = false;
                 """);
    }
    
    protected HttpClient CreateJsonClient()
    {
        HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json;v=1");
        return client;
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
