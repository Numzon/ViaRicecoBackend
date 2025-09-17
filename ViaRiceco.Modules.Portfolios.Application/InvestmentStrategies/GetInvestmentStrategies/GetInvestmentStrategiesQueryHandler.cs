using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.GetInvestmentStrategies;

public sealed record GetInvestmentStrategiesQuery(
    string? Search,
    string? Sort,
    int Page,
    int PageSize,
    string? FinancialGoalId,
    string? InvestmentStrategyTypeId) : IQuery<GetInvestmentStrategiesQueryResponse>;

public sealed record GetInvestmentStrategiesQueryResponse(IReadOnlyCollection<InvestmentStrategyDto> Items, int TotalCount);

internal sealed class GetInvestmentStrategiesQueryHandler(IInvestmentStrategyRepository repository, ISortingService sortingService)
    : IQueryHandler<GetInvestmentStrategiesQuery, GetInvestmentStrategiesQueryResponse>
{
    public async Task<Result<GetInvestmentStrategiesQueryResponse>> Handle(GetInvestmentStrategiesQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<InvestmentStrategyDto, InvestmentStrategy>(request.Sort))
        {
            return Result.Failure<GetInvestmentStrategiesQueryResponse>(InvestmentStrategyErrors.InvalidSortParameter(request.Sort));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<InvestmentStrategyDto, InvestmentStrategy>(request.Sort);
        
        IReadOnlyCollection<InvestmentStrategy> strategies = await repository.GetPageAsync(
            request.Search, 
            orderBy,
            request.Page, 
            request.PageSize,
            request.FinancialGoalId,
            request.InvestmentStrategyTypeId,
            cancellationToken);

        IReadOnlyCollection<InvestmentStrategyDto> dtoCollection = strategies.Select(strategy => 
            new InvestmentStrategyDto(
                strategy.Id,
                strategy.FinancialGoalId,
                strategy.InvestmentStrategyTypeId,
                strategy.UninvestedAmount,
                strategy.TotalInvestedAmount,
                strategy.TotalCurrentAmount,
                strategy.TotalAmount,
                strategy.Investments.Select(inv => new InvestmentSummaryDto(
                    inv.Id,
                    inv.Name,
                    inv.ModelPortfolioPercentage,
                    inv.RealPortfolioPercentage,
                    inv.InvestedAmount,
                    inv.CurrentAmount)).ToList())).ToList();

        int totalCount = await repository.CountAsync(
            request.Search, 
            request.FinancialGoalId, 
            request.InvestmentStrategyTypeId, 
            cancellationToken);
        
        var response = new GetInvestmentStrategiesQueryResponse(dtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
