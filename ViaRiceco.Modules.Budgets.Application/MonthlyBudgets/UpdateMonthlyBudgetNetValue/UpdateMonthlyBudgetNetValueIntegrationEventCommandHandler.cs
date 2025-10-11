using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.UpdateMonthlyBudgetNetValue;

public sealed record UpdateMonthlyBudgetNetValueIntegrationEventCommand(
    string SettlementPeriodId,
    decimal NetAmount) : ICommand;

public sealed class UpdateMonthlyBudgetNetValueIntegrationEventCommandHandler(
    IMonthlyBudgetRepository monthlyBudgetRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<UpdateMonthlyBudgetNetValueIntegrationEventCommand>
{
    public async Task<Result> Handle(UpdateMonthlyBudgetNetValueIntegrationEventCommand request, CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await monthlyBudgetRepository
            .GetBySettlementPeriodIdAsync(request.SettlementPeriodId, cancellationToken);

        if (monthlyBudget == null)
        {
            return Result.Success(); // No monthly budget exists for this settlement period - idempotent
        }

        // If the monthly budget is finalized, set it as draft since the settlement period data changed
        if (!monthlyBudget.IsDraft)
        {
            monthlyBudget.SetAsDraft(timeProvider.UtcNow());
        }

        // Update the net value
        monthlyBudget.UpdateNetValue(request.NetAmount, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
