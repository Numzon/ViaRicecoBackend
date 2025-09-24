using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.IntegrationEvents.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.IntegrationEvents.InvestmentStrategies.Models;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.InvestmentStrategyCreated;

public sealed class InvestmentStrategyCreatedDomainEventHandler(
    IEventBus bus,
    IInvestmentStrategyRepository investmentStrategyRepository,
    IFinancialGoalRepository financialGoalRepository) : DomainEventHandler<InvestmentStrategyCreatedDomainEvent>
{
    public override async Task Handle(InvestmentStrategyCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        InvestmentStrategy? strategy =
            await investmentStrategyRepository.GetAsync(domainEvent.InvestmentStrategyId, cancellationToken);

        ArgumentNullException.ThrowIfNull(strategy);

        IReadOnlyCollection<FinancialGoalTreeElement> elements =
            await financialGoalRepository.GetAllWithChildrenAsTreeAsync(strategy.FinancialGoalId, cancellationToken);

        IReadOnlyCollection<FinancialGoalIntegrationModel> financialGoalIntegrationModels =
            elements.Select(x => new FinancialGoalIntegrationModel
            {
                Id = x.Id,
                Name = x.Name,
                ParentId = x.ParentId,
                TreeDepth = x.TreeDepth
            }).ToList();

        var integrationEvent = new InvestmentStrategyCreatedIntegrationEvent (domainEvent.Id,
            domainEvent.OccurredOnUtc, strategy.Id, 
            strategy.InvestmentStrategyTypeId, financialGoalIntegrationModels);

        await bus.PublishAsync(integrationEvent, cancellationToken);
    }
}
