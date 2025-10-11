using MediatR;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Exceptions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SetMonthlyBudgetAsDraft;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

public sealed class SettlementPeriodSetAsDraftIntegrationEventHandler(
    ISender sender) : IntegrationEventHandler<SettlementPeriodSetAsDraftIntegrationEvent>
{
    public override async Task Handle(SettlementPeriodSetAsDraftIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new SetMonthlyBudgetAsDraftIntegrationEventCommand(integrationEvent.SettlementPeriodId);

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new ViaRicecoException(nameof(SetMonthlyBudgetAsDraftIntegrationEventCommand), result.Error);
        }
    }
}
