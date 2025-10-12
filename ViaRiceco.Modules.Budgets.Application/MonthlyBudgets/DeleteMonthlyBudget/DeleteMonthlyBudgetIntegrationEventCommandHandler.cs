using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.DeleteMonthlyBudget;

internal sealed class DeleteMonthlyBudgetIntegrationEventCommandHandler(
    IMonthlyBudgetRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteMonthlyBudgetIntegrationEventCommand>
{
    public async Task<Result> Handle(DeleteMonthlyBudgetIntegrationEventCommand request,
        CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await repository.GetBySettlementPeriodIdAsync(request.SettlementPeriodId, cancellationToken);

        if (monthlyBudget is null)
        {
            // If MonthlyBudget doesn't exist, consider it success (idempotent)
            return Result.Success();
        }

        repository.Delete(monthlyBudget);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
