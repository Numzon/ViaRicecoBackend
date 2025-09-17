using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;

namespace ViaRiceco.Modules.Portfolios.Application.Currencies.GetCurrencies;

public sealed record GetCurrenciesQuery(string? Search, string? Sort, int Page, int PageSize)
    : IQuery<GetCurrenciesQueryResponse>;

public sealed record GetCurrenciesQueryResponse(IReadOnlyCollection<CurrencyDto> Items, int TotalCount);

internal sealed class GetCurrenciesQueryHandler(ICurrencyRepository repository, ISortingService sortingService)
    : IQueryHandler<GetCurrenciesQuery, GetCurrenciesQueryResponse>
{
    public async Task<Result<GetCurrenciesQueryResponse>> Handle(GetCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<CurrencyDto, Currency>(request.Sort))
        {
            return Result.Failure<GetCurrenciesQueryResponse>(CurrencyErrors.InvalidSortParameter(request.Sort));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<CurrencyDto, Currency>(request.Sort);
        
        IReadOnlyCollection<Currency> currencies = await repository.GetPageAsync(request.Search, orderBy,
            request.Page, request.PageSize, cancellationToken);

        IReadOnlyCollection<CurrencyDto> currencyDtoCollection =
            [.. currencies.Select(currency => new CurrencyDto(currency.Id, currency.Name, currency.Code))];

        int totalCount = await repository.CountAsync(request.Search, cancellationToken);
        
        var response = new GetCurrenciesQueryResponse(currencyDtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
