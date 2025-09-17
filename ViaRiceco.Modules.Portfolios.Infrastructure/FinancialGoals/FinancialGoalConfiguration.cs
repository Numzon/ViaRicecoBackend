using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.FinancialGoals;

internal sealed class FinancialGoalConfiguration : IEntityTypeConfiguration<FinancialGoal>
{
    public void Configure(EntityTypeBuilder<FinancialGoal> builder)
    {
        builder.HasKey(fg => fg.Id);

        builder.Property(fg => fg.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(fg => fg.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(fg => fg.ParentId)
               .HasMaxLength(100);

        builder.Property(fg => fg.CreatedAtUtc)
               .IsRequired();

        builder.Property(fg => fg.UpdatedAtUtc);

        // Self-referencing relationship for parent-child hierarchy
        builder.HasOne<FinancialGoal>()
               .WithMany()
               .HasForeignKey(fg => fg.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on name (within the same parent level)
        builder.HasIndex(fg => new { fg.Name, fg.ParentId })
               .IsUnique();
    }
}
