using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriod;

public sealed record GetSettlementPeriodQuery(string Id) : IQuery<SettlementPeriodDto>;

internal sealed class GetSettlementPeriodQueryHandler(ISettlementPeriodRepository repository)
    : IQueryHandler<GetSettlementPeriodQuery, SettlementPeriodDto>
{
    public async Task<Result<SettlementPeriodDto>> Handle(GetSettlementPeriodQuery request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.Id, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure<SettlementPeriodDto>(SettlementPeriodErrors.NotFound(request.Id));
        }

        var settlementPeriodDto = new SettlementPeriodDto(
            settlementPeriod.Id, 
            settlementPeriod.Month, 
            settlementPeriod.Year,
            settlementPeriod.IsDraft,
            settlementPeriod.TotalIncome,
            settlementPeriod.TotalTaxes,
            settlementPeriod.NetAmount,
            settlementPeriod.CreatedAtUtc,
            settlementPeriod.UpdatedAtUtc,
            settlementPeriod.Incomes.Select(i => new IncomeDto(i.Id, i.Value, i.CreatedAtUtc, i.UpdatedAtUtc)).ToList().AsReadOnly(),
            settlementPeriod.Taxes.Select(t => new TaxDto(t.Id, t.Value, t.TaxTypeId, t.CreatedAtUtc, t.UpdatedAtUtc)).ToList().AsReadOnly());

        return settlementPeriodDto;
    }
}
