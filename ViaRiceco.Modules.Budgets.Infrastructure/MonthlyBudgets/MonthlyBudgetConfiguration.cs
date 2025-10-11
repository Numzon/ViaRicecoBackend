using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgets;

internal sealed class MonthlyBudgetConfiguration : IEntityTypeConfiguration<MonthlyBudget>
{
    public void Configure(EntityTypeBuilder<MonthlyBudget> builder)
    {
        builder.HasKey(mb => mb.Id);

        builder.Property(mb => mb.Id)
               .HasMaxLength(50)
               .ValueGeneratedNever();

        builder.Property(mb => mb.SettlementPeriodId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(mb => mb.Month)
               .IsRequired();

        builder.Property(mb => mb.Year)
               .IsRequired();

        builder.Property(mb => mb.IsDraft)
               .IsRequired();

        builder.Property(mb => mb.CreatedAtUtc)
               .IsRequired();

        builder.Property(mb => mb.UpdatedAtUtc);

        // Indexes
        builder.HasIndex(mb => mb.SettlementPeriodId);
        builder.HasIndex(mb => new { mb.Month, mb.Year });
        builder.HasIndex(mb => mb.IsDraft);
        builder.HasIndex(mb => mb.CreatedAtUtc);

        // Unique constraint on SettlementPeriodId (one budget per settlement period)
        builder.HasIndex(mb => mb.SettlementPeriodId)
               .IsUnique();

        // Configure the relationship with MonthlyBudgetExpenses
        builder.HasMany(mb => mb.Expenses)
               .WithOne()
               .HasForeignKey(mbe => mbe.MonthlyBudgetId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
