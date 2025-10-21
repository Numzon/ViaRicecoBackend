using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Banks.Models;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Application.Banks.GetBank;

public sealed record GetBankQuery(string Id) : IQuery<BankDto>;

internal sealed class GetBankQueryHandler(IBankRepository bankRepository) : IQueryHandler<GetBankQuery, BankDto>
{
    public async Task<Result<BankDto>> Handle(GetBankQuery request, CancellationToken cancellationToken)
    {
        Bank? bank = await bankRepository.GetAsync(request.Id, cancellationToken);
        if (bank is null)
        {
            return Result.Failure<BankDto>(BankErrors.NotFound(request.Id));
        }

        var bankDto = new BankDto(bank.Id, bank.Name);

        return bankDto;
    }
}
