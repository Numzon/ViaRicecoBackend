using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxTypes;

public sealed record GetTaxTypesQuery(string? Search, string? Sort, int Page, int PageSize)
    : IQuery<GetTaxTypesQueryResponse>;

public sealed record GetTaxTypesQueryResponse(IReadOnlyCollection<TaxTypeDto> Items, int TotalCount);

internal sealed class GetTaxTypesQueryHandler(ITaxTypeRepository repository, ISortMappingProvider sortMappingProvider)
    : IQueryHandler<GetTaxTypesQuery, GetTaxTypesQueryResponse>
{
    public async Task<Result<GetTaxTypesQueryResponse>> Handle(GetTaxTypesQuery request,
        CancellationToken cancellationToken)
    {
        if (!sortMappingProvider.ValidateMappings<TaxTypeDto, TaxType>(request.Sort))
        {
            return Result.Failure<GetTaxTypesQueryResponse>(new Error("","",ErrorType.Conflict));
        }
        
        SortMapping[] sortMappings = sortMappingProvider.GetMappings<TaxTypeDto, TaxType>();
        string orderBy = QueryableExtensions.ApplySort(request.Sort, sortMappings);
        
        IReadOnlyCollection<TaxType> taxTypes = await repository.GetPageAsync(request.Search, orderBy,
            request.Page, request.PageSize, cancellationToken);

        IReadOnlyCollection<TaxTypeDto> taxTypeDtoCollection =
            [.. taxTypes.Select(taxType => new TaxTypeDto(taxType.Id, taxType.Name))];

        int totalCount = await repository.CountAsync(cancellationToken);
        
        var response = new GetTaxTypesQueryResponse(taxTypeDtoCollection, totalCount);
        
        return Result.Success(response);
    }
}
