using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SetMonthlyBudgetAsDraft;

internal sealed class SetMonthlyBudgetAsDraftCommandHandler(
    IMonthlyBudgetRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<SetMonthlyBudgetAsDraftCommand>
{
    public async Task<Result> Handle(SetMonthlyBudgetAsDraftCommand request,
        CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await repository.GetAsync(request.Id, cancellationToken);

        if (monthlyBudget is null)
        {
            return Result.Failure(MonthlyBudgetErrors.NotFound(request.Id));
        }

        // Check if this is the most recent monthly budget
        bool isNewerPeriodExists = await repository.IsNewerPeriodExistsAsync(
            monthlyBudget.Month, 
            monthlyBudget.Year, 
            cancellationToken);

        if (isNewerPeriodExists)
        {
            return Result.Failure(MonthlyBudgetErrors.CannotSetAsNonRecentDraft(
                monthlyBudget.Month, 
                monthlyBudget.Year));
        }

        Result result = monthlyBudget.SetAsDraft(timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
