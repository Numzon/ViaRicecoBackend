using Asp.Versioning.Conventions;
using FastEndpoints.AspVersioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;
using ViaRiceco.Modules.Accounting.Infrastructure.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;

namespace ViaRiceco.Modules.Accounting.Infrastructure;

public static class AccountingModule
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration)
                .AddPresentation();
        
        return services;       
    }
    
    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AccountingDbContext>((_, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Database),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Accounting))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<ITaxTypeRepository, TaxTypeRepository>();
        services.AddScoped<ISettlementPeriodRepository, SettlementPeriodRepository>();
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AccountingDbContext>());
        
        return services;
    }

    private static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        VersionSets.CreateApi(CustomVersionSets.TaxTypes, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.SettlementPeriods, v => v
            .HasApiVersion(1.0));
        
        return services;       
    }
}
