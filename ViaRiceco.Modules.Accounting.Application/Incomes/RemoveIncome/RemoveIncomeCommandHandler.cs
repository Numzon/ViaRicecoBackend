using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.Incomes.RemoveIncome;

public sealed record RemoveIncomeCommand(string SettlementPeriodId, string IncomeId) : ICommand;

internal sealed class RemoveIncomeCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RemoveIncomeCommand>
{
    public async Task<Result> Handle(RemoveIncomeCommand request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.SettlementPeriodId, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure(SettlementPeriodErrors.NotFound(request.SettlementPeriodId));
        }

        Result result = settlementPeriod.RemoveIncome(request.IncomeId, timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

