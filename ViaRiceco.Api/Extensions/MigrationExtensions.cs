using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Api.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder builder)
    {
        using IServiceScope scope = builder.ApplicationServices.CreateScope();
        
        ApplyMigrations<AccountingDbContext>(scope);
        ApplyMigrations<PortfoliosDbContext>(scope);
        ApplyMigrations<BudgetsDbContext>(scope);
    }
    
    private static void ApplyMigrations<TDbContext>(IServiceScope serviceScope)
        where TDbContext : DbContext
    {
        using TDbContext context = serviceScope.ServiceProvider.GetRequiredService<TDbContext>();
        
        context.Database.Migrate();
    }
}
