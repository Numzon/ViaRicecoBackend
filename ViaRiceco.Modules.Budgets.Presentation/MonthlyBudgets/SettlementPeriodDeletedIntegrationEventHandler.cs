using MediatR;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Exceptions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.DeleteMonthlyBudget;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

internal sealed class SettlementPeriodDeletedIntegrationEventHandler(
    ISender sender) : IntegrationEventHandler<SettlementPeriodDeletedIntegrationEvent>
{
    public override async Task Handle(SettlementPeriodDeletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteMonthlyBudgetIntegrationEventCommand(integrationEvent.SettlementPeriodId);

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new ViaRicecoException(nameof(DeleteMonthlyBudgetIntegrationEventCommand), result.Error);
        }
    }
}
