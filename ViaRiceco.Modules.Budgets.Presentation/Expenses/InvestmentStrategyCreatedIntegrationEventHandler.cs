using MediatR;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Exceptions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Portfolios.IntegrationEvents;
using ViaRiceco.Modules.Portfolios.IntegrationEvents.InvestmentStrategies;

namespace ViaRiceco.Modules.Budgets.Presentation.Expenses;

internal sealed class InvestmentStrategyCreatedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<InvestmentStrategyCreatedIntegrationEvent>
{
    public override async Task Handle(InvestmentStrategyCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<InvestmentExpenseDto> expenses = integrationEvent.FinancialGoals
            .Select(x => new InvestmentExpenseDto(x.Id, x.Name, x.ParentId, x.TreeDepth))
            .ToList();
        
        var command = new CreateExpenseIntegrationEventCommand(integrationEvent.InvestmentStrategyId, expenses);
        
        Result result = await sender.Send( command, cancellationToken);
        
        if (result.IsFailure)
        {
            throw new ViaRicecoException(nameof(CreateExpenseIntegrationEventCommand), result.Error);
        }
    }
}
