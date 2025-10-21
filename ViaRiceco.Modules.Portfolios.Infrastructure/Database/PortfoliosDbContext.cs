using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Common.Infrastructure.Inbox;
using ViaRiceco.Common.Infrastructure.Outbox;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashRecords;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategyTypes;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Infrastructure.Currencies;
using ViaRiceco.Modules.Portfolios.Infrastructure.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestedCashRecords;
using ViaRiceco.Modules.Portfolios.Infrastructure.Investments;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategyTypes;
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
    internal DbSet<InvestedCashRecord> InvestedCashRecords { get; set; }
    internal DbSet<OutboxMessage> OutboxMessages { get; set; }
    internal DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; set; }
    
    internal DbSet<InboxMessage> InboxMessages { get; set; }
    internal DbSet<InboxMessageConsumer> InboxMessageConsumers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Portfolios);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new FinancialGoalConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentStrategyTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentStrategyConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseRecordConfiguration());
        modelBuilder.ApplyConfiguration(new InvestedCashRecordConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CurrentTransaction.DisposeAsync();
        }

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}
