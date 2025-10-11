using MediatR;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Exceptions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.CreateMonthlyBudget;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

public sealed class SettlementPeriodCreatedIntegrationEventHandler(
    ISender sender) : IntegrationEventHandler<SettlementPeriodCreatedIntegrationEvent>
{
    public override async Task Handle(SettlementPeriodCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateMonthlyBudgetIntegrationEventCommand(integrationEvent.SettlementPeriodId,
            integrationEvent.Month, integrationEvent.Year, integrationEvent.NetAmount);

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new ViaRicecoException(nameof(CreateMonthlyBudgetIntegrationEventCommand), result.Error);
        }
    }
}
