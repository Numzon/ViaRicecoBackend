using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

namespace ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgetExpenses;

internal sealed class MonthlyBudgetExpenseConfiguration : IEntityTypeConfiguration<MonthlyBudgetExpense>
{
    public void Configure(EntityTypeBuilder<MonthlyBudgetExpense> builder)
    {
        builder.HasKey(mbe => mbe.Id);

        builder.Property(mbe => mbe.Id)
               .HasMaxLength(50)
               .ValueGeneratedNever();

        builder.Property(mbe => mbe.MonthlyBudgetId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(mbe => mbe.ExpenseId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(mbe => mbe.ExpenseName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(mbe => mbe.ExpenseTypeId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(mbe => mbe.ExpenseTypeName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(mbe => mbe.Value)
               .HasColumnType("decimal(18,2)");

        builder.Property(mbe => mbe.CreatedAtUtc)
               .IsRequired();

        builder.Property(mbe => mbe.UpdatedAtUtc);

        // Indexes
        builder.HasIndex(mbe => mbe.MonthlyBudgetId);
        builder.HasIndex(mbe => mbe.ExpenseId);
        builder.HasIndex(mbe => mbe.ExpenseTypeId);
        builder.HasIndex(mbe => mbe.CreatedAtUtc);

        // Unique constraint on MonthlyBudgetId + ExpenseId (one expense per budget)
        builder.HasIndex(mbe => new { mbe.MonthlyBudgetId, mbe.ExpenseId })
               .IsUnique();
    }
}
