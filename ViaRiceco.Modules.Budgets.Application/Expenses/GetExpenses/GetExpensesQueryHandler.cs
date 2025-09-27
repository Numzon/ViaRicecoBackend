using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Domain.Expenses;

namespace ViaRiceco.Modules.Budgets.Application.Expenses.GetExpenses;

public sealed record GetExpensesQuery(string? Search, string? Sort, int Page, int PageSize, string? ExpenseTypeId)
    : IQuery<GetExpensesQueryResponse>;

public sealed record GetExpensesQueryResponse(IReadOnlyCollection<ExpenseDto> Items, int TotalCount);

internal sealed class GetExpensesQueryHandler(IExpenseRepository repository, ISortingService sortingService)
    : IQueryHandler<GetExpensesQuery, GetExpensesQueryResponse>
{
    public async Task<Result<GetExpensesQueryResponse>> Handle(GetExpensesQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<ExpenseDto, Expense>(request.Sort))
        {
            return Result.Failure<GetExpensesQueryResponse>(ExpenseErrors.InvalidSortParameter(request.Sort));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<ExpenseDto, Expense>(request.Sort);
        
        IReadOnlyCollection<Expense> expenses = await repository.GetPageAsync(request.Search, orderBy,
            request.Page, request.PageSize, request.ExpenseTypeId, cancellationToken);

        IReadOnlyCollection<ExpenseDto> expenseDtoCollection =
            [.. expenses.Select(expense => new ExpenseDto(expense.Id, expense.Name, expense.ExpenseTypeId))];

        int totalCount = await repository.CountAsync(request.Search, request.ExpenseTypeId, cancellationToken);
        
        var response = new GetExpensesQueryResponse(expenseDtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
