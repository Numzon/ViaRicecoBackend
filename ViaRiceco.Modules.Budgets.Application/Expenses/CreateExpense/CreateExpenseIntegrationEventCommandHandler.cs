using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;

public sealed record InvestmentExpenseDto(string Id, string Name, string? ParentId, int TreeDepth);

public sealed record CreateExpenseIntegrationEventCommand(
    string InvestmentStrategyId,
    IReadOnlyCollection<InvestmentExpenseDto> Expenses) : ICommand;

public sealed class CreateExpenseIntegrationEventCommandHandler(
    IUnitOfWork unitOfWork,
    IExpenseTypeRepository expenseTypeRepository,
    IExpenseRepository expenseRepository,
    TimeProvider timeProvider) : ICommandHandler<CreateExpenseIntegrationEventCommand>
{
    public async Task<Result> Handle(CreateExpenseIntegrationEventCommand request, CancellationToken cancellationToken)
    {
        ExpenseType? expenseType =
            await expenseTypeRepository.GetAsync(ExpenseTypeSpecification.Investment.Id, cancellationToken);

        ArgumentNullException.ThrowIfNull(expenseType);

        int leafDepth = request.Expenses.Max(x => x.TreeDepth);
        var leafs = request.Expenses.Where(x => x.TreeDepth == leafDepth).ToList();
        var leafNames = leafs.Select(x => x.Name).ToList();

        foreach (string leafName in leafNames)
        {
            bool exists =
                await expenseRepository.ExistsByNameAndExpenseTypeAsync(leafName, expenseType.Id,
                    cancellationToken);

            if (exists)
            {
                continue;
            }

            var expense = Expense.CreateFromIntegrationEvent(leafName, expenseType.Id,
                request.InvestmentStrategyId, timeProvider.UtcNow());

            expenseRepository.Insert(expense);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
