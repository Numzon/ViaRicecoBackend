using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.UninvestedMoneyUsed;

internal sealed class UninvestedMoneyUsedDomainEventHandler(
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork,
    IInvestmentStrategyRepository investmentStrategyRepository)
    : DomainEventHandler<PurchaseRecordAddedToInvestmentDomainEvent>
{
    public override async Task Handle(PurchaseRecordAddedToInvestmentDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        InvestmentStrategy? strategy = await investmentStrategyRepository.GetAsync(domainEvent.InvestmentStrategyId, cancellationToken);

        ArgumentNullException.ThrowIfNull(strategy);
        
        strategy.UpdateUninvestedAmount(strategy.UninvestedAmount - domainEvent.TotalAmount, timeProvider.UtcNow());
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
