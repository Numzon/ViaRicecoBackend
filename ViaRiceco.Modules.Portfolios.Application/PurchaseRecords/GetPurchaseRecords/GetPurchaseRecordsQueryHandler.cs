using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.GetPurchaseRecords;

public sealed record GetPurchaseRecordsQuery(
    string? Search, 
    string? Sort, 
    int Page, 
    int PageSize,
    string? InvestmentId,
    string? CurrencyId,
    DateTime? FromDate,
    DateTime? ToDate) : IQuery<GetPurchaseRecordsQueryResponse>;

public sealed record GetPurchaseRecordsQueryResponse(IReadOnlyCollection<PurchaseRecordDto> Items, int TotalCount);

internal sealed class GetPurchaseRecordsQueryHandler(IPurchaseRecordRepository repository)
    : IQueryHandler<GetPurchaseRecordsQuery, GetPurchaseRecordsQueryResponse>
{
    public async Task<Result<GetPurchaseRecordsQueryResponse>> Handle(GetPurchaseRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var baseQueryParameters = new BaseQueryParameters
        {
            Search = request.Search,
            OrderBy = request.Sort ?? "CreatedAtUtc",
            Page = request.Page,
            PageSize = request.PageSize
        };

        var dateRangeParameters = new DateRangeParameters
        {
            FromDate = request.FromDate,
            ToDate = request.ToDate
        };

        IReadOnlyCollection<PurchaseRecord> purchaseRecords = await repository.GetPageAsync(
            baseQueryParameters, 
            dateRangeParameters, 
            request.InvestmentId, 
            request.CurrencyId, 
            cancellationToken);

        IReadOnlyCollection<PurchaseRecordDto> dtoCollection = purchaseRecords.Select(record => 
            new PurchaseRecordDto(
                record.Id,
                record.PurchaseDate,
                record.Amount,
                record.PricePerUnit,
                record.TotalPrice,
                record.CurrencyId,
                record.InvestmentId)).ToList();

        int totalCount = await repository.CountAsync(
            baseQueryParameters, 
            dateRangeParameters, 
            request.InvestmentId, 
            request.CurrencyId, 
            cancellationToken);
        
        var response = new GetPurchaseRecordsQueryResponse(dtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
