using Asp.Versioning.Conventions;
using FastEndpoints.AspVersioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategyTypes;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Infrastructure.Currencies;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using ViaRiceco.Modules.Portfolios.Infrastructure.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Infrastructure.Investments;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategyTypes;
using ViaRiceco.Modules.Portfolios.Infrastructure.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;

namespace ViaRiceco.Modules.Portfolios.Infrastructure;

public static class PortfoliosModule
{
    public static IServiceCollection AddPortfoliosModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration)
                .AddPresentation();
        
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

        // Repository registrations
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IFinancialGoalRepository, FinancialGoalRepository>();
        services.AddScoped<IInvestmentStrategyTypeRepository, InvestmentStrategyTypeRepository>();
        services.AddScoped<IPurchaseRecordRepository, PurchaseRecordRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();
        services.AddScoped<IInvestmentStrategyRepository, InvestmentStrategyRepository>();
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PortfoliosDbContext>());
        
        return services;
    }

    private static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        VersionSets.CreateApi(CustomVersionSets.Currencies, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.FinancialGoals, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.InvestmentStrategies, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.Investments, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.InvestmentStrategyTypes, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.PurchaseRecords, v => v
            .HasApiVersion(1.0));

        return services;
    }
}
