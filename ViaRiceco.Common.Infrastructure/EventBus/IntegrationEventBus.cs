using MassTransit;
using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Common.Infrastructure.EventBus;

internal sealed class IntegrationEventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : IIntegrationEvent
    {
        await bus.Publish(integrationEvent, cancellationToken);
    }
}
