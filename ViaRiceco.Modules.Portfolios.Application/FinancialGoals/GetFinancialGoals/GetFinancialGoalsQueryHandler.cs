using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

namespace ViaRiceco.Modules.Portfolios.Application.FinancialGoals.GetFinancialGoals;

public sealed record GetFinancialGoalsQuery(string? Search, string? Sort, int Page, int PageSize)
    : IQuery<GetFinancialGoalsQueryResponse>;

public sealed record GetFinancialGoalsQueryResponse(IReadOnlyCollection<FinancialGoalDto> Items, int TotalCount);

internal sealed class GetFinancialGoalsQueryHandler(IFinancialGoalRepository repository, ISortingService sortingService)
    : IQueryHandler<GetFinancialGoalsQuery, GetFinancialGoalsQueryResponse>
{
    public async Task<Result<GetFinancialGoalsQueryResponse>> Handle(GetFinancialGoalsQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<FinancialGoalDto, FinancialGoal>(request.Sort))
        {
            return Result.Failure<GetFinancialGoalsQueryResponse>(FinancialGoalErrors.InvalidSortParameter(request.Sort));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<FinancialGoalDto, FinancialGoal>(request.Sort);
        
        IReadOnlyCollection<FinancialGoal> financialGoals = await repository.GetPageAsync(request.Search, orderBy,
            request.Page, request.PageSize, cancellationToken);

        IReadOnlyCollection<FinancialGoalDto> dtoCollection =
            [.. financialGoals.Select(goal => new FinancialGoalDto(goal.Id, goal.Name, goal.ParentId))];

        int totalCount = await repository.CountAsync(request.Search, cancellationToken);
        
        var response = new GetFinancialGoalsQueryResponse(dtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
