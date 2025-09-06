using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure;

public static class PortfoliosModule
{
    public static IServiceCollection AddPortfoliosModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        
        return services;       
    }
    
    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PortfoliosDbContext>((_, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Database),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Portfolios))
                .UseSnakeCaseNamingConvention();
        });

        // Repository registrations will be added here when repositories are created
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PortfoliosDbContext>());
        
        return services;
    }
}
