using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriods;

public sealed record GetSettlementPeriodsQuery(string? Search, string? Sort, int Page, int PageSize, int? Month, int? Year)
    : IQuery<GetSettlementPeriodsQueryResponse>;

public sealed record GetSettlementPeriodsQueryResponse(IReadOnlyCollection<SettlementPeriodDto> Items, int TotalCount);

internal sealed class GetSettlementPeriodsQueryHandler(ISettlementPeriodRepository repository, ISortingService sortingService)
    : IQueryHandler<GetSettlementPeriodsQuery, GetSettlementPeriodsQueryResponse>
{
    public async Task<Result<GetSettlementPeriodsQueryResponse>> Handle(GetSettlementPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<SettlementPeriodDto, SettlementPeriod>(request.Sort))
        {
            return Result.Failure<GetSettlementPeriodsQueryResponse>(
                Error.Validation("SettlementPeriod.InvalidSortParameter", 
                    $"The sort parameter '{request.Sort}' is not valid for settlement periods"));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<SettlementPeriodDto, SettlementPeriod>(request.Sort);
        
        IReadOnlyCollection<SettlementPeriod> settlementPeriods = await repository.GetPageAsync(request.Search, orderBy,
            request.Page, request.PageSize, request.Month, request.Year, cancellationToken);

        IReadOnlyCollection<SettlementPeriodDto> settlementPeriodDtoCollection = settlementPeriods
            .Select(sp => new SettlementPeriodDto(
                sp.Id, 
                sp.Month, 
                sp.Year, 
                sp.TotalIncome,
                sp.TotalTaxes,
                sp.NetAmount,
                sp.CreatedAtUtc,
                sp.UpdatedAtUtc,
                sp.Incomes.Select(i => new IncomeDto(i.Id, i.Value, i.CreatedAtUtc, i.UpdatedAtUtc)).ToList().AsReadOnly(),
                sp.Taxes.Select(t => new TaxDto(t.Id, t.Value, t.TaxTypeId, t.CreatedAtUtc, t.UpdatedAtUtc)).ToList().AsReadOnly()
            )).ToList().AsReadOnly();

        int totalCount = await repository.CountAsync(request.Search, request.Month, request.Year, cancellationToken);
        
        var response = new GetSettlementPeriodsQueryResponse(settlementPeriodDtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
