using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Banks.Models;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Application.Banks.GetBanks;

public sealed record GetBanksQuery(
    string? Search,
    string? Sort,
    int Page,
    int PageSize) : IQuery<GetBanksQueryResponse>;

public sealed record GetBanksQueryResponse(IReadOnlyCollection<BankDto> Items, int TotalCount);

internal sealed class GetBanksQueryHandler(IBankRepository repository, ISortingService sortingService) : IQueryHandler<GetBanksQuery, GetBanksQueryResponse>
{
    public async Task<Result<GetBanksQueryResponse>> Handle(GetBanksQuery request, CancellationToken cancellationToken)
    {
        if (!sortingService.ValidateSortParameters<BankDto, Bank>(request.Sort))
        {
            return Result.Failure<GetBanksQueryResponse>(BankErrors.InvalidSortParameter(request.Sort));
        }
        
        string orderBy = sortingService.GenerateOrderByClause<BankDto, Bank>(request.Sort);

        IReadOnlyCollection<Bank> banks = await repository.GetPageAsync(
            request.Search,
            orderBy,
            request.Page,
            request.PageSize,
            cancellationToken);

        int totalCount = await repository.CountAsync(request.Search, cancellationToken);

        var bankDtos = banks.Select(bank => 
            new BankDto(bank.Id, bank.Name)).ToList();

        var response = new GetBanksQueryResponse(bankDtos, totalCount);

        return response;
    }
}
