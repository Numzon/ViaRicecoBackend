using Asp.Versioning.Conventions;
using FastEndpoints.AspVersioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Common.Infrastructure.Outbox;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.Outbox;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;
using ViaRiceco.Modules.Accounting.Infrastructure.Outbox;
using ViaRiceco.Modules.Accounting.Infrastructure.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;

namespace ViaRiceco.Modules.Accounting.Infrastructure;

public static class AccountingModule
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        
        services.AddInfrastructure(configuration)
                .AddPresentation();
        
        return services;       
    }
    
    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AccountingDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Database),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Accounting))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
        });

        services.AddScoped<ITaxTypeRepository, TaxTypeRepository>();
        services.AddScoped<ISettlementPeriodRepository, SettlementPeriodRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IOutboxMessageConsumerRepository, OutboxMessageConsumerRepository>();
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AccountingDbContext>());

        services.Configure<OutboxOptions>(configuration.GetSection("Accounting:Outbox"));

        services.ConfigureOptions<ConfigureProcessOutboxJob>();
        
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
