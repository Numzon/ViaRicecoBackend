using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.GetInvestmentStrategyTypes;

public sealed record GetInvestmentStrategyTypesQuery(string? Search, string? Sort, int Page, int PageSize)
    : IQuery<GetInvestmentStrategyTypesQueryResponse>;

public sealed record GetInvestmentStrategyTypesQueryResponse(IReadOnlyCollection<InvestmentStrategyTypeDto> Items, int TotalCount);

internal sealed class GetInvestmentStrategyTypesQueryHandler(IInvestmentStrategyTypeRepository repository, ISortingService sortingService)
    : IQueryHandler<GetInvestmentStrategyTypesQuery, GetInvestmentStrategyTypesQueryResponse>
{
    public async Task<Result<GetInvestmentStrategyTypesQueryResponse>> Handle(GetInvestmentStrategyTypesQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<InvestmentStrategyTypeDto, InvestmentStrategyType>(request.Sort))
        {
            return Result.Failure<GetInvestmentStrategyTypesQueryResponse>(InvestmentStrategyTypeErrors.InvalidSortParameter(request.Sort));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<InvestmentStrategyTypeDto, InvestmentStrategyType>(request.Sort);
        
        IReadOnlyCollection<InvestmentStrategyType> investmentStrategyTypes = await repository.GetPageAsync(request.Search, orderBy,
            request.Page, request.PageSize, cancellationToken);

        IReadOnlyCollection<InvestmentStrategyTypeDto> dtoCollection =
            [.. investmentStrategyTypes.Select(type => new InvestmentStrategyTypeDto(type.Id, type.Name))];

        int totalCount = await repository.CountAsync(request.Search, cancellationToken);
        
        var response = new GetInvestmentStrategyTypesQueryResponse(dtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
