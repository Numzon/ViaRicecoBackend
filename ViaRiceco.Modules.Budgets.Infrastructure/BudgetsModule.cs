using Asp.Versioning.Conventions;
using FastEndpoints.AspVersioning;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Common.Infrastructure.Enumerations;
using ViaRiceco.Common.Infrastructure.Outbox;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.Inbox;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.Domain.Outbox;
using ViaRiceco.Modules.Budgets.Infrastructure.Banks;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;
using ViaRiceco.Modules.Budgets.Infrastructure.Expenses;
using ViaRiceco.Modules.Budgets.Infrastructure.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Infrastructure.Inbox;
using ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.Infrastructure.Outbox;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.IntegrationEvents.InvestmentStrategies;

namespace ViaRiceco.Modules.Budgets.Infrastructure;

public static class BudgetsModule
{
    public static IServiceCollection AddBudgetsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        services.AddIntegrationEventHandlers();
        
        services.AddInfrastructure(configuration)
            .AddPresentation();

        return services;
    }
    
    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<InvestmentStrategyCreatedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<SettlementPeriodCreatedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<SettlementPeriodSetAsDraftIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<SettlementPeriodNetAmountRecalculatedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<SettlementPeriodDeletedIntegrationEvent>>();
    }
    
    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BudgetsDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Database),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Budgets))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
        });

        // Repository registrations
        services.AddScoped<IExpenseTypeRepository, ExpenseTypeRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<IMonthlyBudgetRepository, MonthlyBudgetRepository>();
        services.AddScoped<IMonthlyBudgetExpenseRepository, MonthlyBudgetExpenseRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IOutboxMessageConsumerRepository, OutboxMessageConsumerRepository>();
        
        //inbox
        services.AddScoped<IInboxMessageRepository, InboxMessageRepository>();
        services.AddScoped<IInboxMessageConsumerRepository, InboxMessageConsumerRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BudgetsDbContext>());

        services.Configure<OutboxOptions>(configuration.GetSection("Budgets:Outbox"));
        services.Configure<InboxOptions>(configuration.GetSection("Budgets:Inbox"));

        services.ConfigureOptions<ConfigureProcessOutboxJob>();
        services.ConfigureOptions<ConfigureProcessInboxJob>();
        
        return services;
    }

    private static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        VersionSets.CreateApi(CustomVersionSets.ExpenseTypes, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.Expenses, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.Banks, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.MonthlyBudgets, v => v
            .HasApiVersion(1.0));
        
        VersionSets.CreateApi(CustomVersionSets.MonthlyBudgetsExpenses, v => v
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
    
    private static void AddIntegrationEventHandlers(this IServiceCollection services)
    {
        Type[] integrationEventHandlers = Presentation.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToArray();

        foreach (Type integrationEventHandler in integrationEventHandlers)
        {
            services.TryAddScoped(integrationEventHandler);

            Type integrationEvent = integrationEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler =
                typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

            services.Decorate(integrationEventHandler, closedIdempotentHandler);
        }
    }
}

