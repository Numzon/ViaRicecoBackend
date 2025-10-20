using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.Taxes.RemoveTax;

public sealed record RemoveTaxCommand(string SettlementPeriodId, string TaxId) : ICommand;

internal sealed class RemoveTaxCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RemoveTaxCommand>
{
    public async Task<Result> Handle(RemoveTaxCommand request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.SettlementPeriodId, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure(SettlementPeriodErrors.NotFound(request.SettlementPeriodId));
        }

        Result result = settlementPeriod.RemoveTax(request.TaxId, timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

