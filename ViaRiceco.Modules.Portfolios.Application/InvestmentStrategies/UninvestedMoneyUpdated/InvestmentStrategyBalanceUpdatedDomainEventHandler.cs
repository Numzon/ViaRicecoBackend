using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.UninvestedMoneyUpdated;

internal sealed class InvestmentStrategyBalanceUpdatedDomainEventHandler(
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork,
    IInvestmentStrategyRepository investmentStrategyRepository)
    : DomainEventHandler<InvestmentStrategyBalanceUpdatedDomainEvent>
{
    public override async Task Handle(InvestmentStrategyBalanceUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        InvestmentStrategy? strategy = await investmentStrategyRepository
            .GetAsync(domainEvent.InvestmentStrategyId, cancellationToken);

        if (strategy == null)
        {
            return;
        }
        
        strategy.UpdateUninvestedAmount(timeProvider.UtcNow());
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
