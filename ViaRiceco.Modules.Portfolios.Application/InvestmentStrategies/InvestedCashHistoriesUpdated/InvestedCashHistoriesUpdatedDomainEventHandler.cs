using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.InvestedCashHistoriesUpdated;

internal sealed class InvestedCashHistoriesUpdatedDomainEventHandler(
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork,
    IInvestmentStrategyRepository investmentStrategyRepository)
    : DomainEventHandler<InvestedCashHistoriesUpdatedDomainEvent>
{
    public override async Task Handle(InvestedCashHistoriesUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        InvestmentStrategy? strategy = await investmentStrategyRepository
            .GetAsync(domainEvent.InvestmentStrategyId, cancellationToken);

        if (strategy != null)
        {
            decimal uninvestedAmount = CalculateUninvestedAmount(strategy.TotalInvestedCash, strategy.TotalInvestedAmount);
            strategy.UpdateUninvestedAmount(uninvestedAmount, timeProvider.UtcNow());
        }
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private static decimal CalculateUninvestedAmount(decimal totalInvestedCash, decimal totalInvestedAmount)
    {
        return Math.Max(totalInvestedCash - totalInvestedAmount, 0m);
    }
}

