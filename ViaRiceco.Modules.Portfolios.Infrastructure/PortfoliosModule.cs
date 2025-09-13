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
using ViaRiceco.Modules.Portfolios.Domain.Outbox;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Infrastructure.Currencies;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using ViaRiceco.Modules.Portfolios.Infrastructure.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Infrastructure.Investments;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategyTypes;
using ViaRiceco.Modules.Portfolios.Infrastructure.Outbox;
using ViaRiceco.Modules.Portfolios.Infrastructure.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Common.Infrastructure.Outbox;

namespace ViaRiceco.Modules.Portfolios.Infrastructure;

public static class PortfoliosModule
{
    public static IServiceCollection AddPortfoliosModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        
        services.AddInfrastructure(configuration)
            .AddPresentation();

        return services;
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PortfoliosDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Database),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Portfolios))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
        });

        // Repository registrations
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IFinancialGoalRepository, FinancialGoalRepository>();
        services.AddScoped<IInvestmentStrategyTypeRepository, InvestmentStrategyTypeRepository>();
        services.AddScoped<IPurchaseRecordRepository, PurchaseRecordRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();
        services.AddScoped<IInvestmentStrategyRepository, InvestmentStrategyRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IOutboxMessageConsumerRepository, OutboxMessageConsumerRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PortfoliosDbContext>());

        services.Configure<OutboxOptions>(configuration.GetSection("Portfolios:Outbox"));

        services.ConfigureOptions<ConfigureProcessOutboxJob>();
        
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
    
    private static void AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] domainEventHandlers = Application.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToArray();

        foreach (Type domainEventHandler in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandler);

            Type domainEvent = domainEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);
    
            services.Decorate(domainEventHandler, closedIdempotentHandler);
        }
    }
}
