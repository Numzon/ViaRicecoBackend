using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Database;

public sealed class PortfoliosDbContext(DbContextOptions<PortfoliosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    // Entity DbSets will be added here when domain entities are created
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Portfolios);

        // Entity configurations will be applied here when created
    }
}
