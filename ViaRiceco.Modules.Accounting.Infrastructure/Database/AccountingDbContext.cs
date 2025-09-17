using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Common.Infrastructure.Inbox;
using ViaRiceco.Common.Infrastructure.Outbox;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.Infrastructure.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Infrastructure.Database;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<TaxType> TaxTypes { get; set; }
    internal DbSet<SettlementPeriod> SettlementPeriods { get; set; }
    internal DbSet<OutboxMessage> OutboxMessages { get; set; }
    internal DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; set; }
    
    internal DbSet<InboxMessage> InboxMessages { get; set; }
    internal DbSet<InboxMessageConsumer> InboxMessageConsumers { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Accounting);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        
        modelBuilder.ApplyConfiguration(new TaxTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementPeriodConfiguration());
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
