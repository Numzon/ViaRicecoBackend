using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Infrastructure.Currencies;
using ViaRiceco.Modules.Portfolios.Infrastructure.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Infrastructure.Investments;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentTypes;
using ViaRiceco.Modules.Portfolios.Infrastructure.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Database;

public sealed class PortfoliosDbContext(DbContextOptions<PortfoliosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Currency> Currencies { get; set; }
    internal DbSet<FinancialGoal> FinancialGoals { get; set; }
    internal DbSet<InvestmentStrategyType> InvestmentStrategyTypes { get; set; }
    internal DbSet<InvestmentStrategy> InvestmentStrategies { get; set; }
    internal DbSet<Investment> Investments { get; set; }
    internal DbSet<PurchaseRecord> PurchaseRecords { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Portfolios);

        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new FinancialGoalConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentStrategyTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentStrategyConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseRecordConfiguration());
    }
}
