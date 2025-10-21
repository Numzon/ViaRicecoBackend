using MediatR;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Exceptions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.UpdateMonthlyBudgetNetValue;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

internal sealed class SettlementPeriodNetAmountRecalculatedIntegrationEventHandler(
    ISender sender) : IntegrationEventHandler<SettlementPeriodNetAmountRecalculatedIntegrationEvent>
{
    public override async Task Handle(SettlementPeriodNetAmountRecalculatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateMonthlyBudgetNetValueIntegrationEventCommand(
            integrationEvent.SettlementPeriodId,
            integrationEvent.NetAmount);

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new ViaRicecoException(nameof(UpdateMonthlyBudgetNetValueIntegrationEventCommand), result.Error);
        }
    }
}
