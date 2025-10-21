using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Accounting.Domain.Incomes;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.Taxes;

namespace ViaRiceco.Modules.Accounting.Infrastructure.SettlementPeriods;

internal sealed class SettlementPeriodConfiguration : IEntityTypeConfiguration<SettlementPeriod>
{
    public void Configure(EntityTypeBuilder<SettlementPeriod> builder)
    {
        builder.HasKey(sp => sp.Id);
        
        builder.Property(sp => sp.Month)
               .IsRequired()
               .HasAnnotation("CheckConstraint", "Month BETWEEN 1 AND 12");

        builder.Property(sp => sp.Year)
               .IsRequired();

        builder.Property(sp => sp.IsDraft)
               .IsRequired()
               .HasDefaultValue(true); // New settlement periods start as drafts

        // Configure navigation properties for owned collections
        builder.OwnsMany(sp => sp.Incomes, incomes =>
        {
            incomes.WithOwner().HasForeignKey(nameof(Income.SettlementPeriodId));
            incomes.HasKey(i => i.Id);
            
            incomes.Property(i => i.Value)
                   .IsRequired()
                   .HasPrecision(18, 2);
                   
            incomes.Property(i => i.SettlementPeriodId)
                   .IsRequired()
                   .HasMaxLength(100);
        });

        builder.OwnsMany(sp => sp.Taxes, taxes =>
        {
            taxes.WithOwner().HasForeignKey(nameof(Tax.SettlementPeriodId));
            taxes.HasKey(t => t.Id);
            
            taxes.Property(t => t.Value)
                 .IsRequired()
                 .HasPrecision(18, 2);
                 
            taxes.Property(t => t.TaxTypeId)
                 .IsRequired()
                 .HasMaxLength(100);
                 
            taxes.Property(t => t.SettlementPeriodId)
                 .IsRequired()
                 .HasMaxLength(100);
        });

        // Unique constraint on Month and Year combination
        builder.HasIndex(sp => new { sp.Month, sp.Year })
               .IsUnique();

        // Index on IsDraft for efficient querying of draft/finalized periods
        builder.HasIndex(sp => sp.IsDraft);
    }
}
