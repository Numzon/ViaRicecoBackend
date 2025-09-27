using MassTransit;
using Newtonsoft.Json;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Common.Infrastructure.Serialization;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Inbox;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Inbox;

internal sealed class IntegrationEventConsumer<TIntegrationEvent>(
    IUnitOfWork unitOfWork,
    IInboxMessageRepository repository)
    : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    public async Task Consume(ConsumeContext<TIntegrationEvent> context)
    {
        TIntegrationEvent integrationEvent = context.Message;

        var inboxMessage = InboxMessage.Create(integrationEvent.Id, integrationEvent.GetType().Name,
            JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
            integrationEvent.OccurredOnUtc);

        repository.Insert(inboxMessage);        

        await unitOfWork.SaveChangesAsync(context.CancellationToken); 
    }
}
