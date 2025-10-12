using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.SetSettlementPeriodAsDraft;

internal sealed class SetSettlementPeriodAsDraftCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<SetSettlementPeriodAsDraftCommand>
{
    public async Task<Result> Handle(SetSettlementPeriodAsDraftCommand request,
        CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.Id, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure(SettlementPeriodErrors.NotFound(request.Id));
        }

        // Check if this is the most recent settlement period
        bool isNewerPeriodExists = await repository.IsNewerPeriodExistsAsync(
            settlementPeriod.Month, 
            settlementPeriod.Year, 
            cancellationToken);

        if (isNewerPeriodExists)
        {
            return Result.Failure(SettlementPeriodErrors.CannotSetAsNonRecentDraft(
                settlementPeriod.Month, 
                settlementPeriod.Year));
        }

        Result result = settlementPeriod.SetAsDraft(timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
