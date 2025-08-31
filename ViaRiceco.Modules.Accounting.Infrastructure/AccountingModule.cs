using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;
using ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Infrastructure;

public static class AccountingModule
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration);
        
        return services;       
    }

    private static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<TaxTypeDto, TaxType>>(_ => TaxTypeMappings.SortMapping);
        
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
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AccountingDbContext>());
        
        return services;
    }
}
