using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SetMonthlyBudgetAsDraft;

public sealed record SetMonthlyBudgetAsDraftIntegrationEventCommand(string SettlementPeriodId) : ICommand;

public sealed class SetMonthlyBudgetAsDraftIntegrationEventCommandHandler(
    IMonthlyBudgetRepository monthlyBudgetRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<SetMonthlyBudgetAsDraftIntegrationEventCommand>
{
    public async Task<Result> Handle(SetMonthlyBudgetAsDraftIntegrationEventCommand request, CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await monthlyBudgetRepository
            .GetBySettlementPeriodIdAsync(request.SettlementPeriodId, cancellationToken);

        if (monthlyBudget == null)
        {
            return Result.Success(); 
        }

        monthlyBudget.SetAsDraft(timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
