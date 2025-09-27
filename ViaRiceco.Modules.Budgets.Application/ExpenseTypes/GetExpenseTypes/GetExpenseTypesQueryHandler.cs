using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.ExpenseTypes.GetExpenseTypes;

public sealed record GetExpenseTypesQuery(string? Search, string? Sort, int Page, int PageSize)
    : IQuery<GetExpenseTypesQueryResponse>;

public sealed record GetExpenseTypesQueryResponse(IReadOnlyCollection<ExpenseTypeDto> Items, int TotalCount);

internal sealed class GetExpenseTypesQueryHandler(IExpenseTypeRepository repository, ISortingService sortingService)
    : IQueryHandler<GetExpenseTypesQuery, GetExpenseTypesQueryResponse>
{
    public async Task<Result<GetExpenseTypesQueryResponse>> Handle(GetExpenseTypesQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<ExpenseTypeDto, ExpenseType>(request.Sort))
        {
            return Result.Failure<GetExpenseTypesQueryResponse>(ExpenseTypeErrors.InvalidSortParameter(request.Sort));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<ExpenseTypeDto, ExpenseType>(request.Sort);
        
        IReadOnlyCollection<ExpenseType> expenseTypes = await repository.GetPageAsync(request.Search, orderBy,
            request.Page, request.PageSize, cancellationToken);

        IReadOnlyCollection<ExpenseTypeDto> expenseTypeDtoCollection =
            [.. expenseTypes.Select(expenseType => new ExpenseTypeDto(expenseType.Id, expenseType.Name, expenseType.IsSystemDefined))];

        int totalCount = await repository.CountAsync(request.Search, cancellationToken);
        
        var response = new GetExpenseTypesQueryResponse(expenseTypeDtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
