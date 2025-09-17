using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Common.Infrastructure.Inbox;
using ViaRiceco.Common.Infrastructure.Outbox;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Database;

public sealed class BudgetsDbContext(DbContextOptions<BudgetsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<OutboxMessage> OutboxMessages { get; set; }
    internal DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; set; }
    
    internal DbSet<InboxMessage> InboxMessages { get; set; }
    internal DbSet<InboxMessageConsumer> InboxMessageConsumers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Budgets);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException("Transaction is already started");
        }

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}
