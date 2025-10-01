using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Expenses;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(200);
        
        builder.Property(e => e.InvestmentStrategyId)
            .HasMaxLength(100);

        builder.Property(e => e.ExpenseTypeId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.BankId)
               .HasMaxLength(100);

        builder.Property(e => e.IsArchived)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(e => e.CreatedAtUtc)
               .IsRequired();

        builder.Property(e => e.UpdatedAtUtc);

        // Unique constraint on expense name within expense type
        builder.HasIndex(e => new { e.Name, e.ExpenseTypeId })
               .IsUnique();

        // Foreign key relationship to ExpenseType
        builder.HasOne<ExpenseType>()
               .WithMany()
               .HasForeignKey(e => e.ExpenseTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        // Foreign key relationship to Bank (nullable)
        builder.HasOne<Bank>()
               .WithMany()
               .HasForeignKey(e => e.BankId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.InvestmentStrategyId);
        
        builder.HasIndex(e => e.ExpenseTypeId);
        
        builder.HasIndex(e => e.BankId);
        
        builder.HasIndex(e => e.IsArchived);
        
    }
}
