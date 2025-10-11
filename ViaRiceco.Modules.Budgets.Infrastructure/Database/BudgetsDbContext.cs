using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Common.Infrastructure.Inbox;
using ViaRiceco.Common.Infrastructure.Outbox;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Infrastructure.Expenses;
using ViaRiceco.Modules.Budgets.Infrastructure.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Infrastructure.Banks;
using ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgetExpenses;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Database;

public sealed class BudgetsDbContext(DbContextOptions<BudgetsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ExpenseType> ExpenseTypes { get; set; }
    internal DbSet<Expense> Expenses { get; set; }
    internal DbSet<Bank> Banks { get; set; }
    internal DbSet<MonthlyBudget> MonthlyBudgets { get; set; }
    internal DbSet<MonthlyBudgetExpense> MonthlyBudgetExpenses { get; set; }
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
        
        modelBuilder.ApplyConfiguration(new ExpenseTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new BankConfiguration());
        modelBuilder.ApplyConfiguration(new MonthlyBudgetConfiguration());
        modelBuilder.ApplyConfiguration(new MonthlyBudgetExpenseConfiguration());
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

